using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class ColorDefController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ColorDefController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var colors = _context.ColorDefs.OrderBy(c => c.Name).ToList();
            return View(colors);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ColorDef color)
        {
            if (ModelState.IsValid)
            {
                _context.ColorDefs.Add(color);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Renk başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(color);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var color = _context.ColorDefs.Find(id);
            if (color != null)
            {
                _context.ColorDefs.Remove(color);
                _context.SaveChanges();
                return Json(new { success = true, message = "Renk silindi." });
            }
            return Json(new { success = false, message = "Renk bulunamadı." });
        }
    }
}
