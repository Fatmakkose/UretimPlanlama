using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Data;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers
{
    public class ProcessTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProcessTrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
                return RedirectToAction("AccessDenied", "Account");

            var orders = _context.Orders
                .Where(o => o.Status != "İptal Edildi" && o.Status != "Tamamlandı")
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
        public IActionResult UpdateMaterialApproval(int materialId, decimal actualQuantity, bool isApproved)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var material = _context.OrderMaterials.Find(materialId);
            if (material == null) return Json(new { success = false, message = "Malzeme bulunamadı" });

            material.ActualQuantity = actualQuantity;
            material.IsApproved = isApproved;
            
            // Check if all materials are approved to automatically update the global Order status
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

            return Json(new { success = true });
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
            return Json(new { success = true });
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
}
