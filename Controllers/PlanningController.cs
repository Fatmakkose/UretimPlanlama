using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Data;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class PlanningController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlanningController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdatePlan(Order orderData)
        {
            var order = _context.Orders.Find(orderData.Id);
            if (order != null)
            {
                order.FabricArrivalAgreedDate = orderData.FabricArrivalAgreedDate;
                order.FabricArrivalActualDate = orderData.FabricArrivalActualDate;
                order.FabricMeterage = orderData.FabricMeterage;

                order.CuttingStartDate = orderData.CuttingStartDate;
                order.CuttingEndDate = orderData.CuttingEndDate;

                order.SewingWorkshop = orderData.SewingWorkshop;
                order.SewingStartDate = orderData.SewingStartDate;
                order.SewingEndDate = orderData.SewingEndDate;

                order.PackagingStartDate = orderData.PackagingStartDate;
                order.PackagingEndDate = orderData.PackagingEndDate;
                order.LastInspectionDate = orderData.LastInspectionDate;

                order.DepartureDate = orderData.DepartureDate;
                order.WarehouseArrivalDate = orderData.WarehouseArrivalDate;

                order.UnitCost = orderData.UnitCost;
                order.UnitPrice = orderData.UnitPrice;
                
                // Ayrıca kumaşçı da güncellenebilir:
                if (!string.IsNullOrEmpty(orderData.FabricSupplier))
                {
                    order.FabricSupplier = orderData.FabricSupplier;
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Planlama detayları başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
