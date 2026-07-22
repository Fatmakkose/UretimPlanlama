using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using ClosedXML.Excel;

namespace UretimPlanlama.Controllers
{
    [Authorize(Policy = "RaporAccess")]
    public class FinanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            SyncWorkshopsToCariHesaplar();
            var hesaplar = _context.CariHesaplar.OrderByDescending(h => h.OlusturmaTarihi).ToList();
            ViewBag.Orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();
            return View(hesaplar);
        }

        public IActionResult Definitions()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Purchase()
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            SyncWorkshopsToCariHesaplar();
            ViewBag.CariHesaplar = _context.CariHesaplar.Where(c => c.Aktif).OrderBy(c => c.HesapAdi).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();
            ViewBag.Orders = _context.Orders.Where(o => o.Status != "Tamamlandı" && o.Status != "İptal Edildi").OrderByDescending(o => o.OrderDate).ToList();

            // Yeni belge no üretimi (YYYYMM_001 formatında)
            var today = DateTime.Today;
            var prefix = today.ToString("yyyyMM") + "_";
            var lastDoc = _context.CariHareketler
                .Where(h => h.IslemTipi == "Alış" && h.BelgeNo != null && h.BelgeNo.StartsWith(prefix))
                .OrderByDescending(h => h.BelgeNo)
                .Select(h => h.BelgeNo)
                .FirstOrDefault();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastDoc))
            {
                var numStr = lastDoc.Substring(prefix.Length);
                if (int.TryParse(numStr, out int lastNum))
                {
                    nextNum = lastNum + 1;
                }
            }
            ViewBag.YeniBelgeNo = $"{prefix}{nextNum:D3}";

            return View();
        }

        [HttpPost]
        public IActionResult SavePurchase([FromBody] CariHareketRequest model)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var cari = _context.CariHesaplar.Find(model.CariHesapId);
                if (cari == null) return Json(new { success = false, message = "Cari hesap bulunamadı." });

                // Alış işleminde cari bakiye artar (Tedarikçiye olan borcumuz artar)
                cari.Bakiye += model.Tutar;
                _context.CariHesaplar.Update(cari);

                string finalAciklama = model.Aciklama ?? $"{model.BelgeNo} nolu alış işlemi";
                if (model.OrderId.HasValue)
                {
                    var order = _context.Orders.Find(model.OrderId.Value);
                    if (order != null)
                    {
                        var colorStr = !string.IsNullOrWhiteSpace(order.Color) ? $" - {order.Color}" : "";
                        var orderInfo = $"[Sipariş: {order.OrderCode} - {order.ModelName}{colorStr}]";
                        finalAciklama = string.IsNullOrEmpty(model.Aciklama) 
                            ? orderInfo 
                            : $"{model.Aciklama} {orderInfo}";
                    }
                }

                var cariHareket = new CariHareket
                {
                    CariHesapId = model.CariHesapId,
                    IslemTarihi = model.IslemTarihi,
                    IslemTipi = "Alış",
                    Aciklama = finalAciklama,
                    BelgeNo = model.BelgeNo,
                    Tutar = model.Tutar,
                    KalanBakiye = cari.Bakiye,
                    OrderId = model.OrderId
                };
                _context.CariHareketler.Add(cariHareket);

                if (model.StokKalemleri != null && model.StokKalemleri.Any())
                {
                    foreach (var kalem in model.StokKalemleri)
                    {
                        var stok = _context.StokKartlari.Find(kalem.StokKartiId);
                        if (stok != null)
                        {
                            stok.MevcutMiktar += kalem.Miktar;

                            int? finalVaryantId = kalem.StokVaryantId;
                            if (!string.IsNullOrEmpty(kalem.VaryantAdi))
                            {
                                var varyantName = kalem.VaryantAdi.Trim();
                                var varyant = _context.StokVaryantlar.FirstOrDefault(v => v.StokKartiId == stok.Id && v.VaryantAdi == varyantName);
                                if (varyant == null)
                                {
                                    varyant = new StokVaryant { StokKartiId = stok.Id, VaryantAdi = varyantName, MevcutMiktar = kalem.Miktar };
                                    _context.StokVaryantlar.Add(varyant);
                                    _context.SaveChanges();
                                    finalVaryantId = varyant.Id;
                                }
                                else
                                {
                                    varyant.MevcutMiktar += kalem.Miktar;
                                    _context.StokVaryantlar.Update(varyant);
                                    finalVaryantId = varyant.Id;
                                }
                            }
                            else if (kalem.StokVaryantId.HasValue && kalem.StokVaryantId > 0)
                            {
                                var varyant = _context.StokVaryantlar.Find(kalem.StokVaryantId.Value);
                                if (varyant != null)
                                {
                                    varyant.MevcutMiktar += kalem.Miktar;
                                }
                            }
                            
                            // İsterseniz son alış fiyatını güncelleyebilirsiniz:
                            if (kalem.BirimFiyat.HasValue && kalem.BirimFiyat > 0)
                            {
                                stok.BirimFiyat = kalem.BirimFiyat;
                            }
                            
                            _context.StokKartlari.Update(stok);

                            var stokHareket = new StokHareket
                            {
                                StokKartiId = stok.Id,
                                StokVaryantId = finalVaryantId,
                                IslemTarihi = model.IslemTarihi,
                                HareketTipi = "Giriş",
                                Miktar = kalem.Miktar,
                                KalanMiktar = stok.MevcutMiktar,
                                Aciklama = finalAciklama,
                                BelgeNo = model.BelgeNo,
                                Tedarikci = cari.HesapAdi,
                                OrderId = model.OrderId
                            };
                            _context.StokHareketler.Add(stokHareket);
                        }
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
                return Json(new { success = true, message = "Alış işlemi başarıyla kaydedildi.", newBelgeNo = model.BelgeNo });
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        public IActionResult Sales()
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            SyncWorkshopsToCariHesaplar();
            ViewBag.CariHesaplar = _context.CariHesaplar.Where(c => c.Aktif).OrderBy(c => c.HesapAdi).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Include(s => s.Varyantlar).Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();
            ViewBag.Orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();

            // Yeni belge no üretimi (YYYYMM_001 formatında - Alış ile aynı format/ortak havuz)
            var today = DateTime.Today;
            var prefix = today.ToString("yyyyMM") + "_";
            var lastDoc = _context.CariHareketler
                .Where(h => h.BelgeNo != null && h.BelgeNo.StartsWith(prefix))
                .OrderByDescending(h => h.BelgeNo)
                .Select(h => h.BelgeNo)
                .FirstOrDefault();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastDoc))
            {
                var numStr = lastDoc.Substring(prefix.Length);
                if (int.TryParse(numStr, out int lastNum))
                {
                    nextNum = lastNum + 1;
                }
            }
            ViewBag.YeniBelgeNo = $"{prefix}{nextNum:D3}";

            return View();
        }

        [HttpPost]
        public IActionResult SaveSales([FromBody] CariHareketRequest model)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var cari = _context.CariHesaplar.Find(model.CariHesapId);
                if (cari == null) return Json(new { success = false, message = "Cari hesap bulunamadı." });

                // Satış işleminde cari bakiye azalır
                cari.Bakiye -= model.Tutar;
                _context.CariHesaplar.Update(cari);

                string finalAciklama = model.Aciklama ?? $"{model.BelgeNo} nolu satış işlemi";
                if (model.OrderId.HasValue && model.OrderId.Value > 0)
                {
                    var order = _context.Orders.Find(model.OrderId.Value);
                    if (order != null)
                    {
                        var colorStr = !string.IsNullOrWhiteSpace(order.Color) ? $" - {order.Color}" : "";
                        var orderInfo = $"[Sipariş: {order.OrderCode} - {order.ModelName}{colorStr}]";
                        finalAciklama = string.IsNullOrEmpty(model.Aciklama) 
                            ? orderInfo 
                            : $"{model.Aciklama} {orderInfo}";
                    }
                }

                var cariHareket = new CariHareket
                {
                    CariHesapId = model.CariHesapId,
                    IslemTarihi = model.IslemTarihi,
                    IslemTipi = "Satış",
                    Aciklama = finalAciklama,
                    BelgeNo = model.BelgeNo,
                    Tutar = model.Tutar,
                    KalanBakiye = cari.Bakiye,
                    OrderId = model.OrderId
                };
                _context.CariHareketler.Add(cariHareket);

                if (model.StokKalemleri != null && model.StokKalemleri.Any())
                {
                    foreach (var kalem in model.StokKalemleri)
                    {
                        var stok = _context.StokKartlari.Find(kalem.StokKartiId);
                        if (stok != null)
                        {
                            stok.MevcutMiktar -= kalem.Miktar; // Satışta stok düşer

                            if (kalem.StokVaryantId.HasValue && kalem.StokVaryantId > 0)
                            {
                                var varyant = _context.StokVaryantlar.Find(kalem.StokVaryantId.Value);
                                if (varyant != null)
                                {
                                    varyant.MevcutMiktar -= kalem.Miktar;
                                }
                            }

                            _context.StokKartlari.Update(stok);

                            var stokHareket = new StokHareket
                            {
                                StokKartiId = stok.Id,
                                StokVaryantId = kalem.StokVaryantId > 0 ? kalem.StokVaryantId : null,
                                IslemTarihi = model.IslemTarihi,
                                HareketTipi = "Çıkış",
                                Miktar = kalem.Miktar,
                                KalanMiktar = stok.MevcutMiktar,
                                Aciklama = $"{model.BelgeNo} nolu belge ile satış çıkışı",
                                BelgeNo = model.BelgeNo,
                                OrderId = model.OrderId,
                                Tedarikci = cari.HesapAdi
                            };
                            _context.StokHareketler.Add(stokHareket);
                        }
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
                return Json(new { success = true, message = "Satış işlemi başarıyla kaydedildi.", newBelgeNo = model.BelgeNo });
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        public IActionResult Reports()
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            var hareketler = _context.CariHareketler
                .Include(h => h.CariHesap)
                .OrderByDescending(h => h.IslemTarihi)
                .ThenByDescending(h => h.Id)
                .ToList();
                
            return View(hareketler);
        }

        [HttpGet]
        public IActionResult GetCariDetail(int id)
        {
            var hesap = _context.CariHesaplar.Find(id);
            if (hesap == null)
                return Json(new { success = false, message = "Cari hesap bulunamadı." });

            var hareketler = _context.CariHareketler
                .Where(h => h.CariHesapId == id)
                .OrderByDescending(h => h.IslemTarihi)
                .Select(h => new
                {
                    h.Id,
                    IslemTarihi = h.IslemTarihi.ToString("dd.MM.yyyy"),
                    h.IslemTipi,
                    h.Aciklama,
                    h.Tutar,
                    h.KalanBakiye,
                    h.BelgeNo,
                    h.OrderId,
                    h.EFaturaYolu
                })
                .ToList();

            return Json(new { success = true, hesap = hesap, hareketler = hareketler });
        }

        [HttpPost]
        public IActionResult CreateCariHesap([FromBody] CariHesap model)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            if (string.IsNullOrEmpty(model.HesapAdi))
                return Json(new { success = false, message = "Hesap adı zorunludur." });

            try
            {
                // Otomatik hesap kodu oluştur
                if (string.IsNullOrEmpty(model.HesapKodu))
                {
                    var lastCode = _context.CariHesaplar
                        .OrderByDescending(h => h.Id)
                        .Select(h => h.HesapKodu)
                        .FirstOrDefault();

                    int nextNum = 1;
                    if (!string.IsNullOrEmpty(lastCode) && lastCode.StartsWith("CRH-"))
                    {
                        int.TryParse(lastCode.Replace("CRH-", ""), out nextNum);
                        nextNum++;
                    }
                    model.HesapKodu = $"CRH-{nextNum:D4}";
                }

                model.OlusturmaTarihi = DateTime.Now;
                model.Bakiye = 0;
                _context.CariHesaplar.Add(model);
                _context.SaveChanges();
                return Json(new { success = true, message = "Cari hesap başarıyla oluşturuldu.", hesap = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EditCariHesap([FromBody] CariHesap model)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            try
            {
                var existing = _context.CariHesaplar.Find(model.Id);
                if (existing == null)
                    return Json(new { success = false, message = "Cari hesap bulunamadı." });

                existing.HesapAdi = model.HesapAdi;
                existing.HesapTipi = model.HesapTipi;
                existing.Telefon = model.Telefon;
                existing.Email = model.Email;
                existing.VergiDairesi = model.VergiDairesi;
                existing.VergiNumarasi = model.VergiNumarasi;
                existing.Adres = model.Adres;
                existing.Aktif = model.Aktif;

                _context.SaveChanges();
                return Json(new { success = true, message = "Cari hesap güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateHareket([FromForm] CariHareket model, IFormFile? faturaDosyasi, [FromForm] string? stokKalemleriJson)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            try
            {
                var hesap = _context.CariHesaplar.Find(model.CariHesapId);
                if (hesap == null)
                    return Json(new { success = false, message = "Cari hesap bulunamadı." });

                if (model.Tutar <= 0)
                    return Json(new { success = false, message = "Tutar sıfırdan büyük olmalıdır." });

                if (faturaDosyasi != null && faturaDosyasi.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "efaturalar");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(faturaDosyasi.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        faturaDosyasi.CopyTo(stream);
                    }

                    model.EFaturaYolu = "/uploads/efaturalar/" + uniqueFileName;
                }

                model.IslemTarihi = model.IslemTarihi == default ? DateTime.Now : model.IslemTarihi;

                // Bakiyeyi güncelle
                if (model.IslemTipi == "Alacak" || model.IslemTipi == "Satış")
                    hesap.Bakiye += model.Tutar;
                else // Borç veya Alış
                    hesap.Bakiye -= model.Tutar;

                model.KalanBakiye = hesap.Bakiye;

                _context.CariHareketler.Add(model);

                // Alış Faturası ise stok hareketi oluştur
                if (model.IslemTipi == "Alış Faturası" || model.IslemTipi == "Alış" || model.IslemTipi == "Borç")
                {
                    // Eski tekli stok mantığı
                    if (model.StokKartiId.HasValue && model.Miktar.HasValue)
                    {
                        var stokKarti = _context.StokKartlari.Find(model.StokKartiId.Value);
                        if (stokKarti != null)
                        {
                            stokKarti.MevcutMiktar += model.Miktar.Value;

                            var stokHareket = new StokHareket
                            {
                                StokKartiId = stokKarti.Id,
                                HareketTipi = "Giriş",
                                Miktar = model.Miktar.Value,
                                IslemTarihi = model.IslemTarihi,
                                Aciklama = "Cari Alış (" + hesap.HesapAdi + ")",
                                BelgeNo = model.BelgeNo,
                                OrderId = model.OrderId,
                                KalanMiktar = stokKarti.MevcutMiktar
                            };
                            _context.StokHareketler.Add(stokHareket);
                        }
                    }

                    // Yeni çoklu stok kalemleri mantığı (JSON)
                    if (!string.IsNullOrEmpty(stokKalemleriJson))
                    {
                        try
                        {
                            var kalemler = System.Text.Json.JsonSerializer.Deserialize<List<StokKalemDto>>(stokKalemleriJson);
                            if (kalemler != null)
                            {
                                foreach(var kalem in kalemler)
                                {
                                    if (kalem.StokKartiId > 0 && kalem.Miktar > 0)
                                    {
                                        var stokKarti = _context.StokKartlari.Find(kalem.StokKartiId);
                                        if (stokKarti != null)
                                        {
                                            stokKarti.MevcutMiktar += kalem.Miktar;
                                            var stokHareket = new StokHareket
                                            {
                                                StokKartiId = stokKarti.Id,
                                                HareketTipi = "Giriş",
                                                Miktar = kalem.Miktar,
                                                IslemTarihi = model.IslemTarihi,
                                                Aciklama = "Sipariş Bağlantılı Alış (" + hesap.HesapAdi + ")",
                                                BelgeNo = model.BelgeNo,
                                                OrderId = model.OrderId,
                                                KalanMiktar = stokKarti.MevcutMiktar
                                            };
                                            _context.StokHareketler.Add(stokHareket);
                                        }
                                    }
                                }
                            }
                        }
                        catch { /* JSON Parse hatası yok sayılır */ }
                    }
                }

                _context.SaveChanges();
                return Json(new { success = true, message = "Hareket kaydedildi.", yeniBakiye = hesap.Bakiye });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteTransaction(int id)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var cariHareket = _context.CariHareketler.Find(id);
                if (cariHareket == null)
                    return Json(new { success = false, message = "Cari hareket bulunamadı." });

                // 1. Cari Bakiye Geri Al
                var cariHesap = _context.CariHesaplar.Find(cariHareket.CariHesapId);
                if (cariHesap != null)
                {
                    if (cariHareket.IslemTipi == "Alış")
                        cariHesap.Bakiye -= cariHareket.Tutar;
                    else if (cariHareket.IslemTipi == "Satış")
                        cariHesap.Bakiye += cariHareket.Tutar;
                    else if (cariHareket.IslemTipi == "Alacak")
                        cariHesap.Bakiye -= cariHareket.Tutar;
                    else if (cariHareket.IslemTipi == "Borç")
                        cariHesap.Bakiye += cariHareket.Tutar;
                    
                    _context.CariHesaplar.Update(cariHesap);
                }

                // 2. İlgili Stok Hareketlerini Bul ve Stokları Geri Al
                if (!string.IsNullOrEmpty(cariHareket.BelgeNo))
                {
                    var stokHareketler = _context.StokHareketler.Where(sh => sh.BelgeNo == cariHareket.BelgeNo).ToList();
                    foreach (var sh in stokHareketler)
                    {
                        var stok = _context.StokKartlari.Find(sh.StokKartiId);
                        if (stok != null)
                        {
                            if (sh.HareketTipi == "Giriş")
                                stok.MevcutMiktar -= sh.Miktar;
                            else if (sh.HareketTipi == "Çıkış")
                                stok.MevcutMiktar += sh.Miktar;
                                
                            _context.StokKartlari.Update(stok);
                        }

                        if (sh.StokVaryantId.HasValue)
                        {
                            var varyant = _context.StokVaryantlar.Find(sh.StokVaryantId.Value);
                            if (varyant != null)
                            {
                                if (sh.HareketTipi == "Giriş")
                                    varyant.MevcutMiktar -= sh.Miktar;
                                else if (sh.HareketTipi == "Çıkış")
                                    varyant.MevcutMiktar += sh.Miktar;
                                
                                _context.StokVaryantlar.Update(varyant);
                            }
                        }

                        _context.StokHareketler.Remove(sh);
                    }
                }

                _context.CariHareketler.Remove(cariHareket);
                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, message = "Cari hareket ve bağlı stok hareketleri başarıyla silindi." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteCariHesap(int id)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            try
            {
                var hesap = _context.CariHesaplar.Include(h => h.Hareketler).FirstOrDefault(h => h.Id == id);
                if (hesap == null)
                    return Json(new { success = false, message = "Cari hesap bulunamadı." });

                _context.CariHesaplar.Remove(hesap);
                _context.SaveChanges();
                return Json(new { success = true, message = "Cari hesap silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetBakiyeOzet()
        {
            var hesaplar = _context.CariHesaplar.Where(h => h.Aktif).ToList();

            var toplamAlacak = hesaplar.Where(h => h.Bakiye > 0).Sum(h => h.Bakiye);
            var toplamBorc = hesaplar.Where(h => h.Bakiye < 0).Sum(h => Math.Abs(h.Bakiye));
            var netBakiye = hesaplar.Sum(h => h.Bakiye);
            var aktifHesapSayisi = hesaplar.Count;

            return Json(new
            {
                toplamAlacak,
                toplamBorc,
                netBakiye,
                aktifHesapSayisi
            });
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            var hareketler = _context.CariHareketler
                .Include(h => h.CariHesap)
                .OrderByDescending(h => h.IslemTarihi)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Cari Hareketler");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Hesap Kodu";
                worksheet.Cell(currentRow, 2).Value = "Hesap Adı";
                worksheet.Cell(currentRow, 3).Value = "İşlem Tarihi";
                worksheet.Cell(currentRow, 4).Value = "İşlem Tipi";
                worksheet.Cell(currentRow, 5).Value = "Belge No";
                worksheet.Cell(currentRow, 6).Value = "Açıklama";
                worksheet.Cell(currentRow, 7).Value = "Tutar (₺)";
                worksheet.Cell(currentRow, 8).Value = "Kalan Bakiye (₺)";

                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                foreach (var h in hareketler)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = h.CariHesap?.HesapKodu ?? "";
                    worksheet.Cell(currentRow, 2).Value = h.CariHesap?.HesapAdi ?? "";
                    worksheet.Cell(currentRow, 3).Value = h.IslemTarihi.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 4).Value = h.IslemTipi;
                    worksheet.Cell(currentRow, 5).Value = h.BelgeNo ?? "";
                    worksheet.Cell(currentRow, 6).Value = h.Aciklama ?? "";
                    worksheet.Cell(currentRow, 7).Value = h.Tutar;
                    worksheet.Cell(currentRow, 8).Value = h.KalanBakiye;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CariHareketler.xlsx");
                }
            }
        }

        private void SyncWorkshopsToCariHesaplar()
        {
            try
            {
                var workshops = _context.Workshops.ToList();
                if (!workshops.Any()) return;

                bool hasChanges = false;
                var existingCaris = _context.CariHesaplar.ToList();

                int maxNum = 0;
                foreach (var c in existingCaris)
                {
                    if (!string.IsNullOrEmpty(c.HesapKodu) && c.HesapKodu.StartsWith("CRH-"))
                    {
                        if (int.TryParse(c.HesapKodu.Replace("CRH-", ""), out int num))
                        {
                            if (num > maxNum) maxNum = num;
                        }
                    }
                }

                foreach (var ws in workshops)
                {
                    if (string.IsNullOrWhiteSpace(ws.Name)) continue;
                    var wsName = ws.Name.Trim();

                    var matchingCari = existingCaris.FirstOrDefault(c => c.HesapAdi.Equals(wsName, StringComparison.OrdinalIgnoreCase));
                    if (matchingCari == null)
                    {
                        maxNum++;
                        while (existingCaris.Any(c => c.HesapKodu == $"CRH-{maxNum:D4}"))
                        {
                            maxNum++;
                        }

                        var newCari = new CariHesap
                        {
                            HesapKodu = $"CRH-{maxNum:D4}",
                            HesapAdi = wsName,
                            HesapTipi = "Fason Atölye",
                            Telefon = ws.AuthorizedPerson,
                            Adres = ws.Address,
                            Aktif = ws.IsActive,
                            OlusturmaTarihi = DateTime.Now,
                            Bakiye = 0
                        };
                        _context.CariHesaplar.Add(newCari);
                        existingCaris.Add(newCari);
                        hasChanges = true;
                    }
                    else
                    {
                        if (matchingCari.Aktif != ws.IsActive)
                        {
                            matchingCari.Aktif = ws.IsActive;
                            _context.CariHesaplar.Update(matchingCari);
                            hasChanges = true;
                        }
                    }
                }

                if (hasChanges)
                {
                    _context.SaveChanges();
                }
            }
            catch { }
        }
    }
}
