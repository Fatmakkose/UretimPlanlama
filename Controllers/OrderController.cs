using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
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

        public IActionResult Create()
        {
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                order.Status = "Yeni Kayıt";
                order.FabricStatus = "Bekleniyor";
                _context.Add(order);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Yeni sipariş başarıyla eklendi.";
                return RedirectToAction(nameof(Index)); // Redirect directly to the order management page
            }
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                return Json(new { success = true, message = "Sipariş durumu güncellendi." });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }

        [HttpPost]
        public IActionResult UpdateFabricStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.FabricStatus = status;
                _context.SaveChanges();
                return Json(new { success = true, message = "Kumaş durumu güncellendi." });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }
    }
}
