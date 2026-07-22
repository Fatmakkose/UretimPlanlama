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
}
