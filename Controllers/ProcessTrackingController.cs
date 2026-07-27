using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Data;
using UretimPlanlama.Models;

using Microsoft.AspNetCore.Authorization;

namespace UretimPlanlama.Controllers
{
    [Authorize(Policy = "SurecAccess")]
    public class ProcessTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UretimPlanlama.Services.IEmailService _emailService;

        public ProcessTrackingController(ApplicationDbContext context, UretimPlanlama.Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            var orders = _context.Orders
                .Where(o => o.Status != "İptal Edildi")
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        public IActionResult Track(int id)
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            var order = _context.Orders
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokKarti)
                        .ThenInclude(s => s.Varyantlar)
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokVaryant)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound("Sipariş bulunamadı");

            ViewBag.AllOrders = _context.Orders
                .Where(o => o.Status != "İptal Edildi" && o.Status != "Tamamlandı")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.StokKartlari = _context.StokKartlari.ToList();
            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            
            var salesMovements = _context.StokHareketler
                .Include(sh => sh.StokKarti)
                .Where(sh => sh.OrderId == id && sh.HareketTipi == "Çıkış")
                .OrderByDescending(sh => sh.IslemTarihi)
                .ToList();
            ViewBag.SalesMovements = salesMovements;

            var purchaseMovements = _context.StokHareketler
                .Include(sh => sh.StokKarti)
                .Where(sh => sh.OrderId == id && sh.HareketTipi == "Giriş")
                .ToList();
            ViewBag.PurchaseMovements = purchaseMovements;

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdatePurchasingApproval(int Id, bool IsPurchasingApproved)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(Id);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.IsPurchasingApproved = IsPurchasingApproved;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateMaterialApproval(int materialId, string actualQuantityStr, bool isApproved, int? selectedVaryantId)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var material = _context.OrderMaterials
                .Include(m => m.StokKarti)
                .Include(m => m.StokVaryant)
                .Include(m => m.Order)
                .FirstOrDefault(m => m.Id == materialId);
            if (material == null) return Json(new { success = false, message = "Malzeme bulunamadı" });

            decimal actualQuantity = 0;
            if (!string.IsNullOrEmpty(actualQuantityStr)) {
                actualQuantityStr = actualQuantityStr.Replace(",", ".");
                decimal.TryParse(actualQuantityStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out actualQuantity);
            }

            if (material.IsApproved != isApproved)
            {
                if (isApproved && selectedVaryantId.HasValue && selectedVaryantId > 0)
                {
                    material.StokVaryantId = selectedVaryantId;
                    material.StokVaryant = _context.StokVaryantlar.Find(selectedVaryantId.Value);
                }

                if (isApproved)
                {
                    decimal siparisAlisMiktar = _context.StokHareketler
                        .Where(sh => sh.OrderId == material.OrderId && sh.HareketTipi == "Giriş" && sh.StokKartiId == material.StokKartiId)
                        .Sum(sh => (decimal?)sh.Miktar) ?? 0;

                    decimal rawStock = material.StokVaryant != null ? material.StokVaryant.MevcutMiktar : (material.StokKarti != null ? material.StokKarti.MevcutMiktar : 0);
                    decimal availableStock = Math.Max(siparisAlisMiktar, Math.Max(0, rawStock));
                    
                    if (availableStock < actualQuantity)
                    {
                        return Json(new { success = false, message = "Yetersiz stok. Lütfen önce alış faturası ile depoya giriş yapınız." });
                    }

                    if (material.StokVaryant != null && material.StokVaryant.MevcutMiktar >= actualQuantity) 
                        material.StokVaryant.MevcutMiktar -= actualQuantity;
                    else if (material.StokVaryant != null)
                        material.StokVaryant.MevcutMiktar = 0;

                    if (material.StokKarti != null && material.StokKarti.MevcutMiktar >= actualQuantity) 
                        material.StokKarti.MevcutMiktar -= actualQuantity;
                    else if (material.StokKarti != null)
                        material.StokKarti.MevcutMiktar = 0;

                    material.ActualQuantity = actualQuantity;

                    string extraFeatures = "";
                    if (!string.IsNullOrEmpty(material.OzelliklerJson))
                    {
                        try {
                            using var doc = System.Text.Json.JsonDocument.Parse(material.OzelliklerJson);
                            var parts = new System.Collections.Generic.List<string>();
                            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
                                foreach(var i in doc.RootElement.EnumerateArray()) {
                                    if (i.TryGetProperty("Key", out var k) && i.TryGetProperty("Value", out var v)) parts.Add($"{k.GetString()}: {v.GetString()}");
                                }
                            } else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
                                foreach(var prop in doc.RootElement.EnumerateObject()) parts.Add($"{prop.Name}: {prop.Value.GetString()}");
                            }
                            if (parts.Count > 0) extraFeatures = " [" + string.Join(" | ", parts) + "]";
                        } catch {}
                    }

                    var hareket = new StokHareket {
                        StokKartiId = material.StokKartiId,
                        StokVaryantId = material.StokVaryantId,
                        IslemTarihi = DateTime.Now,
                        HareketTipi = "Çıkış",
                        Miktar = actualQuantity,
                        Aciklama = $"Sipariş Planlama Tahsisi - Otomatik Çıkış (Sipariş: {material.Order?.OrderCode ?? material.OrderId.ToString()}){extraFeatures}",
                        OrderId = material.OrderId,
                        Tedarikci = "KANUNİ TEKSTİL",
                        KalanMiktar = (material.StokVaryant != null ? material.StokVaryant.MevcutMiktar : (material.StokKarti != null ? material.StokKarti.MevcutMiktar : 0))
                    };
                    _context.StokHareketler.Add(hareket);
                }
                else
                {
                    if (material.StokVaryant != null) material.StokVaryant.MevcutMiktar += material.ActualQuantity;
                    if (material.StokKarti != null) material.StokKarti.MevcutMiktar += material.ActualQuantity;

                    string extraFeatures = "";
                    if (!string.IsNullOrEmpty(material.OzelliklerJson))
                    {
                        try {
                            using var doc = System.Text.Json.JsonDocument.Parse(material.OzelliklerJson);
                            var parts = new System.Collections.Generic.List<string>();
                            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
                                foreach(var i in doc.RootElement.EnumerateArray()) {
                                    if (i.TryGetProperty("Key", out var k) && i.TryGetProperty("Value", out var v)) parts.Add($"{k.GetString()}: {v.GetString()}");
                                }
                            } else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
                                foreach(var prop in doc.RootElement.EnumerateObject()) parts.Add($"{prop.Name}: {prop.Value.GetString()}");
                            }
                            if (parts.Count > 0) extraFeatures = " [" + string.Join(" | ", parts) + "]";
                        } catch {}
                    }

                    var hareket = new StokHareket {
                        StokKartiId = material.StokKartiId,
                        StokVaryantId = material.StokVaryantId,
                        IslemTarihi = DateTime.Now,
                        HareketTipi = "Giriş",
                        Miktar = material.ActualQuantity,
                        Aciklama = $"Sipariş Planlama İptali - İade Girişi (Sipariş: {material.Order?.OrderCode ?? material.OrderId.ToString()}){extraFeatures}",
                        OrderId = material.OrderId,
                        Tedarikci = "KANUNİ TEKSTİL",
                        KalanMiktar = (material.StokVaryant != null ? material.StokVaryant.MevcutMiktar : (material.StokKarti != null ? material.StokKarti.MevcutMiktar : 0))
                    };
                    _context.StokHareketler.Add(hareket);

                    material.ActualQuantity = 0;
                }
                
                material.IsApproved = isApproved;
                
                _context.SaveChanges();

                var order = _context.Orders
                    .Include(o => o.OrderMaterials)
                    .FirstOrDefault(o => o.Id == material.OrderId);
                    
                if (order != null)
                {
                    bool allApproved = order.OrderMaterials.All(m => m.IsApproved);
                    if (order.IsPurchasingApproved != allApproved)
                    {
                        order.IsPurchasingApproved = allApproved;
                        _context.SaveChanges();
                    }
                }
            }

            return Json(new { success = true, newStock = material.StokVaryant != null ? material.StokVaryant.MevcutMiktar : material.StokKarti?.MevcutMiktar });
        }

        [HttpPost]
        public IActionResult UpdateMaterialDispatch([FromBody] MaterialDispatchRequest request)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(request.Id);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.MaterialDispatchJson = request.MaterialDispatchJson;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateCuttingProcess([FromBody] CuttingProcessRequest request)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(request.Id);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.CuttingProcessJson = request.CuttingProcessJson;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult SaveFileClosing(int orderId, string fileClosingJson, bool completeOrder)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(orderId);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.FileClosingJson = fileClosingJson;
            
            if (completeOrder)
            {
                order.Status = "Tamamlandı";
            }

            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult MarkTimelineCompleted(int orderId, string key, string type)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(orderId);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            if (key == "sample_kumas_ytesti")
            {
                var talosDict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.TalosTestJson))
                {
                    try { talosDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.TalosTestJson) ?? new Dictionary<string, string>(); } catch {}
                }
                string GetT(string k) => talosDict.ContainsKey(k) ? talosDict[k] : "";
                
                bool step5 = GetT("talos_step5") == "true" || GetT("talos_kalir_approval_status") == "EVET_OK";
                bool step4 = GetT("talos_step4") == "true" || GetT("talos_kalir_val_status") == "VAR_KALIR";
                bool step3 = GetT("talos_step3") == "true" || (GetT("talos_test_result_status") != "" && GetT("talos_test_result_status") != "BEKLENIYOR");

                bool isTalosApproved = step5 || (step3 && !step4);
                
                if (!isTalosApproved)
                {
                    return Json(new { success = false, message = "TALOS Kumaş Test değerleri tam olarak onaylanmadan Kumaş Y-Testi onayı (gerçekleşen tarih) girilemez! Lütfen önce tablodaki TALOS onaylarını tamamlayıp kaydediniz." });
                }
            }

            string targetKey = key + "_actual";
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            if (type == "sample")
            {
                var dict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.SampleTestJson)) {
                    try { dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.SampleTestJson); } catch {}
                }
                dict[targetKey] = today;
                order.SampleTestJson = System.Text.Json.JsonSerializer.Serialize(dict);
            }
            else if (type == "prod")
            {
                var dict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.ProductionJson)) {
                    try { dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.ProductionJson); } catch {}
                }
                dict[targetKey] = today;
                order.ProductionJson = System.Text.Json.JsonSerializer.Serialize(dict);
            }

            string title = "Süreç Güncellemesi";
            string typeName = "Genel";
            string formattedToday = DateTime.Now.ToString("dd.MM.yyyy");
            string message = $"{order.OrderCode} nolu sipariş için işlem {formattedToday} olarak tamamlandı.";

            switch (key)
            {
                case "sample_kumas_ytesti": title = "Numune Kumaş Y-Testi"; typeName = "Kumaş"; message = $"{order.OrderCode} nolu sipariş için Kumaş Y-Testi onayı {formattedToday} olarak verildi."; break;
                case "sample_tuse_renk": title = "Numune Tuşe/Renk"; typeName = "Kumaş"; message = $"{order.OrderCode} nolu sipariş için Kumaş Tuşe/Renk onayı {formattedToday} olarak verildi."; break;
                case "sample_dugme_renk": title = "Numune Düğme/Renk"; typeName = "Aksesuar"; message = $"{order.OrderCode} nolu sipariş için Düğme Renk Kalite onayı {formattedToday} olarak verildi."; break;
                case "sample_pp_onay": title = "PP Onay"; typeName = "Genel"; message = $"{order.OrderCode} nolu sipariş için PP Onay {formattedToday} olarak verildi."; break;
                
                case "prod_kesim_baslangic": title = "Kesim Başladı"; typeName = "Kesim"; message = $"{order.OrderCode} nolu sipariş için kesim başlangıcı {formattedToday} olarak girildi."; break;
                case "prod_kesim_bitis": title = "Kesim Bitti"; typeName = "Kesim"; message = $"{order.OrderCode} nolu sipariş için kesim bitişi {formattedToday} olarak girildi."; break;
                case "prod_dikim_baslangic": title = "Dikim Başladı"; typeName = "Dikim"; message = $"{order.OrderCode} nolu sipariş için dikim başlangıcı {formattedToday} olarak girildi."; break;
                case "prod_dikim_bitis": title = "Dikim Bitti"; typeName = "Dikim"; message = $"{order.OrderCode} nolu sipariş için dikim bitişi {formattedToday} olarak girildi."; break;
                case "prod_paket_baslangic": title = "Paketleme Başladı"; typeName = "Paket"; message = $"{order.OrderCode} nolu sipariş için paketleme başlangıcı {formattedToday} olarak girildi."; break;
                case "prod_paket_bitis": title = "Paketleme Bitti"; typeName = "Paket"; message = $"{order.OrderCode} nolu sipariş için paketleme bitişi {formattedToday} olarak girildi."; break;
                
                case "prod_gs_gidisi": title = "GS Gidişi"; typeName = "Sevkiyat"; message = $"{order.OrderCode} nolu sipariş için GS Gidişi {formattedToday} olarak girildi."; break;
                case "prod_yola_cikis": title = "Yola Çıkış"; typeName = "Sevkiyat"; message = $"{order.OrderCode} nolu sipariş yola çıktı ({formattedToday})."; break;
                case "prod_depo_varis": title = "Depo Varış"; typeName = "Sevkiyat"; message = $"{order.OrderCode} nolu sipariş depoya ulaştı ({formattedToday})."; break;
                
                case "termin_tarihi": title = "Sipariş Tamamlandı"; typeName = "Genel"; message = $"{order.OrderCode} nolu sipariş termin hedefine ulaştı."; break;
            }

            _context.Notifications.Add(new Notification
            {
                Title = title,
                Message = message,
                Type = typeName,
                OrderCode = order.OrderCode,
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            _context.SaveChanges();

            // Send notification email asynchronously
            var users = _context.Users.Where(u => !string.IsNullOrEmpty(u.Email) && u.ReceiveEmailNotifications).Select(u => u.Email).ToList();
            if (users.Any())
            {
                string targetEmails = string.Join(",", users);
                string subject = $"Süreç Bildirimi: {order.OrderCode} - {title}";
                string body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h3 style='color: #0f766e;'>{title}</h3>
                        <p>{message}</p>
                        <p style='color: #64748b; font-size: 0.9em; margin-top: 20px;'>Bu e-posta sistem tarafından otomatik gönderilmiştir.</p>
                    </div>";
                
                // Fire and forget so it doesn't block the UI response
                _ = _emailService.SendEmailAsync(targetEmails, subject, body);
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateFileClosing([FromBody] FileClosingRequest request)
        {
            if (!User.HasPermission("Edit"))
                return Json(new { success = false, message = "Yetkiniz yok" });

            var order = _context.Orders.FirstOrDefault(o => o.Id == request.Id);
            if (order == null)
                return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.FileClosingJson = request.FileClosingJson;
            _context.SaveChanges();
            
            return Json(new { success = true, message = "Dosya Kapama verileri kaydedildi" });
        }

        [HttpPost]
        public IActionResult ProcessSevkOnayStockExit(int orderId)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz işlem." });

            var order = _context.Orders
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokKarti)
                        .ThenInclude(s => s.Varyantlar)
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokVaryant)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı." });

            var purchaseMovements = _context.StokHareketler
                .Where(sh => sh.OrderId == orderId && sh.HareketTipi == "Giriş")
                .ToList();

            var existingExits = _context.StokHareketler
                .Where(sh => sh.OrderId == orderId && sh.HareketTipi == "Çıkış")
                .ToList();

            var purDataTrack = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(order.PurchasingMaterialsJson))
            {
                try { purDataTrack = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.PurchasingMaterialsJson) ?? new Dictionary<string, string>(); } catch { }
            }

            var trackColors = new List<string>();
            if (!string.IsNullOrEmpty(order.Color))
            {
                trackColors = order.Color.Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim().ToUpper()).ToList();
            }
            if (!trackColors.Any()) trackColors.Add("GENEL");

            Func<string, double> parseTr = (string v) => {
                if (string.IsNullOrWhiteSpace(v)) return 0;
                v = v.Trim();
                if (v.Contains(",") && v.Contains(".")) v = v.Replace(".", "").Replace(",", ".");
                else if (v.Contains(",")) v = v.Replace(",", ".");
                else if (v.Contains(".") && v.IndexOf(".") == v.Length - 4) v = v.Replace(".", "");
                double.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double r);
                return r;
            };

            Func<string, string, string?> extractFromOzellikler = (string json, string keyContains) => {
                if (string.IsNullOrEmpty(json)) return null;
                try {
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
                        foreach(var item in doc.RootElement.EnumerateArray()) {
                            if (item.TryGetProperty("Key", out var k) && item.TryGetProperty("Value", out var v)) {
                                if (k.GetString() != null && k.GetString()!.Contains(keyContains, StringComparison.OrdinalIgnoreCase)) return v.GetString();
                            }
                        }
                    } else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
                        foreach(var prop in doc.RootElement.EnumerateObject()) {
                            if (prop.Name.Contains(keyContains, StringComparison.OrdinalIgnoreCase)) {
                                return prop.Value.GetString();
                            }
                        }
                    }
                } catch {}
                return null;
            };

            int deductedCount = 0;
            int alreadyDeductedCount = 0;
            int insufficientCount = 0;
            var resultList = new List<object>();

            foreach (var mat in order.OrderMaterials)
            {
                var stokKarti = mat.StokKarti;
                string dynBoyut = extractFromOzellikler(mat.OzelliklerJson, "BOYUT") ?? extractFromOzellikler(mat.OzelliklerJson, "EBAT") ?? extractFromOzellikler(mat.OzelliklerJson, "BÜYÜKLÜK") ?? extractFromOzellikler(mat.OzelliklerJson, "BOY");
                string extraInfo = !string.IsNullOrEmpty(dynBoyut) ? dynBoyut : mat.Aciklama;

                string dynMatBirim = extractFromOzellikler(mat.OzelliklerJson, "TELA BİRİM") 
                    ?? extractFromOzellikler(mat.OzelliklerJson, "BİRİM KULLANIM") 
                    ?? extractFromOzellikler(mat.OzelliklerJson, "BİRİM") 
                    ?? extractFromOzellikler(mat.OzelliklerJson, "BIRIM");

                if (!string.IsNullOrEmpty(dynMatBirim)) {
                    if (string.IsNullOrEmpty(extraInfo)) {
                        extraInfo = $"BİRİM: {dynMatBirim}";
                    } else if (!extraInfo.Contains("BİRİM", StringComparison.OrdinalIgnoreCase) && !extraInfo.Contains("BIRIM", StringComparison.OrdinalIgnoreCase)) {
                        extraInfo = $"{extraInfo} | BİRİM: {dynMatBirim}";
                    }
                }

                string matName = !string.IsNullOrEmpty(extraInfo) ? $"{stokKarti?.StokAdi ?? "Bilinmeyen Malzeme"} ({extraInfo})" : (stokKarti?.StokAdi ?? "Belirtilmemiş Malzeme");

                double totalIstenen = 0;
                string itemKey = "stk_" + mat.StokKartiId;
                string cat = (stokKarti?.Kategori ?? "").Trim().ToUpperInvariant();

                foreach(var c in trackColors) {
                    string sipAdetiStr = purDataTrack.ContainsKey($"pur_color_{c}_siparis_adeti") ? purDataTrack[$"pur_color_{c}_siparis_adeti"] : order.CalculatedQuantity.ToString();
                    double siparisAdeti = parseTr(sipAdetiStr);
                    if (siparisAdeti == 0) siparisAdeti = order.CalculatedQuantity;

                    string bedenMiktarStr = purDataTrack.ContainsKey($"pur_color_{c}_beden_miktar") ? purDataTrack[$"pur_color_{c}_beden_miktar"] : "0";
                    double bedenMiktar = parseTr(bedenMiktarStr);

                    string bedenCikacakStr = purDataTrack.ContainsKey($"pur_color_{c}_beden_cikacak") ? purDataTrack[$"pur_color_{c}_beden_cikacak"] : "0";
                    double bedenCikacak = parseTr(bedenCikacakStr);

                    double jobQty = bedenMiktar > 0 ? bedenCikacak : siparisAdeti;
                    
                    if (cat == "KUMAŞ" || cat == "KUMAS") {
                        string kumasIstenenStr = purDataTrack.ContainsKey($"pur_color_{c}_beden_miktar") ? purDataTrack[$"pur_color_{c}_beden_miktar"] : null;
                        double dKumas = 0;
                        if (kumasIstenenStr != null) {
                            dKumas = parseTr(kumasIstenenStr);
                        }
                        
                        if (dKumas == 0) {
                            string kumasIhtiyacStr = purDataTrack.ContainsKey($"pur_color_{c}_istenen_kumas_miktar") ? purDataTrack[$"pur_color_{c}_istenen_kumas_miktar"] : null;
                            if (kumasIhtiyacStr != null) {
                                dKumas = parseTr(kumasIhtiyacStr);
                            }
                        }
                        
                        if (dKumas > 0) {
                            totalIstenen += dKumas;
                        } else {
                            string metrajStr = extractFromOzellikler(mat.OzelliklerJson, "METRAJ") ?? mat.Miktar.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            double metraj = 0; double.TryParse(metrajStr.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out metraj);
                            totalIstenen += Math.Round(jobQty * metraj);
                        }
                    } else if (cat == "TELA") {
                        string dynBirim = extractFromOzellikler(mat.OzelliklerJson, "TELA BİRİM") ?? extractFromOzellikler(mat.OzelliklerJson, "BİRİM");
                        string defBirim = !string.IsNullOrEmpty(dynBirim) ? dynBirim : mat.Miktar.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        string birimStr = purDataTrack.ContainsKey($"pur_color_{c}_tela_birim_{itemKey}") ? purDataTrack[$"pur_color_{c}_tela_birim_{itemKey}"] : defBirim;
                        double birim = 0; double.TryParse(birimStr.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out birim);
                        totalIstenen += Math.Ceiling(jobQty * birim);
                    } else if (cat == "ETİKET" || cat == "ETIKET") {
                        string dynFire = extractFromOzellikler(mat.OzelliklerJson, "FİRE (%)") ?? extractFromOzellikler(mat.OzelliklerJson, "FIRE") ?? extractFromOzellikler(mat.OzelliklerJson, "FİRE");
                        string defFire = !string.IsNullOrEmpty(dynFire) ? dynFire : "0";
                        string fireStr = purDataTrack.ContainsKey($"pur_color_{c}_label_fire_{itemKey}") ? purDataTrack[$"pur_color_{c}_label_fire_{itemKey}"] : defFire;
                        double fire = 0; double.TryParse(fireStr.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out fire);
                        double tmp = jobQty * 1; 
                        totalIstenen += Math.Round(tmp + Math.Round(tmp * (fire / 100.0)));
                    } else {
                        string dynBirim = extractFromOzellikler(mat.OzelliklerJson, "BİRİM KULLANIM") ?? extractFromOzellikler(mat.OzelliklerJson, "BİRİM") ?? extractFromOzellikler(mat.OzelliklerJson, "BIRIM");
                        string defBirim = !string.IsNullOrEmpty(dynBirim) ? dynBirim : mat.Miktar.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        string birimStr = purDataTrack.ContainsKey($"pur_color_{c}_birim_{itemKey}") ? purDataTrack[$"pur_color_{c}_birim_{itemKey}"] : defBirim;
                        double birim = 0; double.TryParse(birimStr.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out birim);
                        
                        string dynFire = extractFromOzellikler(mat.OzelliklerJson, "FİRE (%)") ?? extractFromOzellikler(mat.OzelliklerJson, "FIRE") ?? extractFromOzellikler(mat.OzelliklerJson, "FİRE");
                        string defFire = !string.IsNullOrEmpty(dynFire) ? dynFire : (order.WastageRate ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        string fireStr = purDataTrack.ContainsKey($"pur_color_{c}_fire_{itemKey}") ? purDataTrack[$"pur_color_{c}_fire_{itemKey}"] : defFire;
                        double fire = 0; double.TryParse(fireStr.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out fire);
                        
                        double toplam = jobQty * birim;
                        double fazla = Math.Round(toplam * (fire / 100.0));
                        totalIstenen += Math.Round(toplam + fazla);
                    }
                }

                double planMiktar = totalIstenen > 0 ? totalIstenen : (double)mat.Miktar;

                double siparisAlisMiktar = 0;
                if (mat.StokVaryantId.HasValue && mat.StokVaryantId > 0) {
                    siparisAlisMiktar = (double)purchaseMovements.Where(p => p.StokKartiId == mat.StokKartiId && p.StokVaryantId == mat.StokVaryantId).Sum(p => p.Miktar);
                } else if (!string.IsNullOrEmpty(mat.OzelliklerJson) && stokKarti != null && stokKarti.Varyantlar != null) {
                    var vMatch = stokKarti.Varyantlar.FirstOrDefault(v => v.VaryantAdi == mat.OzelliklerJson.Trim());
                    if (vMatch != null) {
                        siparisAlisMiktar = (double)purchaseMovements.Where(p => p.StokKartiId == mat.StokKartiId && p.StokVaryantId == vMatch.Id).Sum(p => p.Miktar);
                    }
                }
                if (siparisAlisMiktar == 0) {
                    siparisAlisMiktar = (double)purchaseMovements.Where(p => p.StokKartiId == mat.StokKartiId).Sum(p => p.Miktar);
                }

                double rawDepoStok = 0;
                if (mat.StokVaryant != null) {
                    rawDepoStok = (double)mat.StokVaryant.MevcutMiktar;
                } else if (!string.IsNullOrEmpty(mat.OzelliklerJson) && stokKarti != null && stokKarti.Varyantlar != null) {
                    var vMatch = stokKarti.Varyantlar.FirstOrDefault(v => v.VaryantAdi == mat.OzelliklerJson.Trim());
                    if (vMatch != null) {
                        rawDepoStok = (double)vMatch.MevcutMiktar;
                    } else if (stokKarti != null) {
                        rawDepoStok = (double)stokKarti.MevcutMiktar;
                    }
                } else if (stokKarti != null) {
                    rawDepoStok = (double)stokKarti.MevcutMiktar;
                }
                double mevcutDepoStok = Math.Max(0, rawDepoStok);
                double sevkMiktar = Math.Max(siparisAlisMiktar, mevcutDepoStok);
                bool isStokYeterli = sevkMiktar >= planMiktar;

                // Daha önce çıkış yapılmış mı kontrol et
                bool alreadyExited = existingExits.Any(e => e.StokKartiId == mat.StokKartiId && (mat.StokVaryantId == null || e.StokVaryantId == mat.StokVaryantId)) || mat.IsApproved;

                string statusStr = "";
                double cikanMiktar = 0;

                if (isStokYeterli)
                {
                    if (!alreadyExited)
                    {
                        decimal decMiktar = (decimal)planMiktar;
                        if (mat.StokVaryant != null)
                        {
                            mat.StokVaryant.MevcutMiktar = Math.Max(0, mat.StokVaryant.MevcutMiktar - decMiktar);
                        }
                        if (mat.StokKarti != null)
                        {
                            mat.StokKarti.MevcutMiktar = Math.Max(0, mat.StokKarti.MevcutMiktar - decMiktar);
                        }

                        mat.ActualQuantity = decMiktar;
                        mat.IsApproved = true;

                        var hareket = new StokHareket
                        {
                            StokKartiId = mat.StokKartiId,
                            StokVaryantId = mat.StokVaryantId,
                            IslemTarihi = DateTime.Now,
                            HareketTipi = "Çıkış",
                            Miktar = decMiktar,
                            Aciklama = $"Sevk Onay Depo Çıkışı (Sipariş: {order.OrderCode})",
                            OrderId = order.Id,
                            Tedarikci = "KANUNİ TEKSTİL",
                            KalanMiktar = (mat.StokVaryant != null ? mat.StokVaryant.MevcutMiktar : (mat.StokKarti != null ? mat.StokKarti.MevcutMiktar : 0)),
                            IsApproved = true
                        };
                        _context.StokHareketler.Add(hareket);
                        deductedCount++;
                        cikanMiktar = planMiktar;
                        statusStr = "DÜŞÜM YAPILDI";
                    }
                    else
                    {
                        alreadyDeductedCount++;
                        cikanMiktar = (double)mat.ActualQuantity > 0 ? (double)mat.ActualQuantity : planMiktar;
                        statusStr = "ÖNCEDEN DÜŞÜLDÜ";
                    }
                }
                else
                {
                    insufficientCount++;
                    statusStr = "YETERSİZ STOK";
                }

                double kalanDepoStok = mat.StokVaryant != null ? (double)mat.StokVaryant.MevcutMiktar : (mat.StokKarti != null ? (double)mat.StokKarti.MevcutMiktar : 0);

                resultList.Add(new {
                    MaterialName = matName,
                    PlanMiktar = planMiktar,
                    SevkMiktar = sevkMiktar,
                    CikanMiktar = cikanMiktar,
                    KalanStok = kalanDepoStok,
                    IsStokYeterli = isStokYeterli,
                    Status = statusStr
                });
            }

            _context.SaveChanges();

            return Json(new {
                success = true,
                message = $"{deductedCount} malzeme stoktan düşüldü, {alreadyDeductedCount} malzeme önceden düşülmüştü, {insufficientCount} malzeme yetersiz.",
                deductedCount = deductedCount,
                alreadyDeductedCount = alreadyDeductedCount,
                insufficientCount = insufficientCount,
                materials = resultList,
                orderCode = order.OrderCode,
                modelName = order.ModelName,
                customer = order.Customer,
                date = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
            });
        }

        [HttpPost]
        public IActionResult SaveTalosTest([FromBody] TalosTestRequest request)
        {
            if (!User.HasPermission("Write") && !User.HasPermission("Edit"))
                return Json(new { success = false, message = "Yetkiniz yok" });

            var order = _context.Orders.FirstOrDefault(o => o.Id == request.Id);
            if (order == null)
                return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.TalosTestJson = request.TalosTestJson;
            _context.SaveChanges();

            return Json(new { success = true, message = "Kumaş Testleri (TALOS) kaydedildi." });
        }

        [HttpPost]
        public IActionResult UpdatePackingList([FromBody] PackingListRequest request)
        {
            if (!User.HasPermission("Write") && !User.HasPermission("Edit"))
                return Json(new { success = false, message = "Yetkiniz yok" });

            var order = _context.Orders.FirstOrDefault(o => o.Id == request.Id);
            if (order == null)
                return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.PackingListJson = request.PackingListJson;
            _context.SaveChanges();

            return Json(new { success = true, message = "Çeki Listesi kaydedildi." });
        }
    }

    public class MaterialDispatchRequest
    {
        public int Id { get; set; }
        public string MaterialDispatchJson { get; set; } = string.Empty;
    }

    public class CuttingProcessRequest
    {
        public int Id { get; set; }
        public string CuttingProcessJson { get; set; } = string.Empty;
    }

    public class FileClosingRequest
    {
        public int Id { get; set; }
        public string FileClosingJson { get; set; } = string.Empty;
    }

    public class TalosTestRequest
    {
        public int Id { get; set; }
        public string TalosTestJson { get; set; } = string.Empty;
    }

    public class PackingListRequest
    {
        public int Id { get; set; }
        public string PackingListJson { get; set; } = string.Empty;
    }
}
