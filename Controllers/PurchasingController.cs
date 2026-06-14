using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using System.Linq;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class PurchasingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var activeOrders = _context.Orders
                .Where(o => o.Status != "Tamamlandı" && o.Status != "İptal Edildi")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(activeOrders);
        }
    }
}
