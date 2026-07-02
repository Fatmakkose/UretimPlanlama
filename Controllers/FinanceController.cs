using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using ClosedXML.Excel;

namespace UretimPlanlama.Controllers
{
    [Authorize]
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

            ViewBag.CariHesaplar = _context.CariHesaplar.Where(c => c.Aktif).OrderBy(c => c.HesapAdi).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();

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

                var cariHareket = new CariHareket
                {
                    CariHesapId = model.CariHesapId,
                    IslemTarihi = model.IslemTarihi,
                    IslemTipi = "Alış",
                    Aciklama = model.Aciklama ?? $"{model.BelgeNo} nolu alış işlemi",
                    BelgeNo = model.BelgeNo,
                    Tutar = model.Tutar,
                    KalanBakiye = cari.Bakiye
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
                            
                            // İsterseniz son alış fiyatını güncelleyebilirsiniz:
                            if (kalem.BirimFiyat.HasValue && kalem.BirimFiyat > 0)
                            {
                                stok.BirimFiyat = kalem.BirimFiyat;
                            }
                            
                            _context.StokKartlari.Update(stok);

                            var stokHareket = new StokHareket
                            {
                                StokKartiId = stok.Id,
                                IslemTarihi = model.IslemTarihi,
                                HareketTipi = "Giriş",
                                Miktar = kalem.Miktar,
                                KalanMiktar = stok.MevcutMiktar,
                                Aciklama = $"{model.BelgeNo} nolu belge ile alış girişi",
                                BelgeNo = model.BelgeNo,
                                Tedarikci = cari.HesapAdi
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

            ViewBag.CariHesaplar = _context.CariHesaplar.Where(c => c.Aktif).OrderBy(c => c.HesapAdi).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();
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

                var cariHareket = new CariHareket
                {
                    CariHesapId = model.CariHesapId,
                    IslemTarihi = model.IslemTarihi,
                    IslemTipi = "Satış",
                    Aciklama = model.Aciklama ?? $"{model.BelgeNo} nolu satış işlemi",
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
                            _context.StokKartlari.Update(stok);

                            var stokHareket = new StokHareket
                            {
                                StokKartiId = stok.Id,
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
        public IActionResult DeleteHareket(int id)
        {
            if (!User.HasPermission("Write"))
                return Json(new { success = false, message = "Yetkiniz yetersiz." });

            try
            {
                var hareket = _context.CariHareketler.Find(id);
                if (hareket == null)
                    return Json(new { success = false, message = "Hareket bulunamadı." });

                var hesap = _context.CariHesaplar.Find(hareket.CariHesapId);
                if (hesap != null)
                {
                    // Hareketi geri al
                    if (hareket.IslemTipi == "Alacak")
                        hesap.Bakiye -= hareket.Tutar;
                    else
                        hesap.Bakiye += hareket.Tutar;
                }

                // Eğer bu hareket bir Alış Faturası ise stokları da geri al
                if (hareket.IslemTipi == "Alış Faturası" && hareket.StokKartiId.HasValue && hareket.Miktar.HasValue)
                {
                    var stokKarti = _context.StokKartlari.Find(hareket.StokKartiId.Value);
                    if (stokKarti != null)
                    {
                        stokKarti.MevcutMiktar -= hareket.Miktar.Value;
                        
                        // İlgili Stok Hareketini de sil (aynı tarih, belge no, miktar, stok kartı eşleşen ilk kayıt)
                        var ilgiliStokHareketi = _context.StokHareketler.FirstOrDefault(sh => sh.StokKartiId == stokKarti.Id && sh.BelgeNo == hareket.BelgeNo && sh.Miktar == hareket.Miktar.Value && sh.HareketTipi == "Giriş");
                        if (ilgiliStokHareketi != null)
                        {
                            _context.StokHareketler.Remove(ilgiliStokHareketi);
                        }
                    }
                }

                _context.CariHareketler.Remove(hareket);
                _context.SaveChanges();
                return Json(new { success = true, message = "Hareket silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
    }
}
