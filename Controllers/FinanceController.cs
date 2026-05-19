using Microsoft.AspNetCore.Mvc;

namespace UretimPlanlama.Controllers
{
    public class FinanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
