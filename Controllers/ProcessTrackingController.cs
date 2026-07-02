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
            ViewBag.Workshops = _context.Workshops.ToList();
            
            var salesMovements = _context.StokHareketler
                .Include(sh => sh.StokKarti)
                .Where(sh => sh.OrderId == id && sh.HareketTipi == "Çıkış")
                .OrderByDescending(sh => sh.IslemTarihi)
                .ToList();
            ViewBag.SalesMovements = salesMovements;

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
