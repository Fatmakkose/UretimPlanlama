using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using System.Linq;

namespace UretimPlanlama.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DefinitionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DefinitionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Workshops()
        {
            var workshops = _context.Workshops.ToList();
            return View(workshops);
        }

        public IActionResult CreateWorkshop()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateWorkshop(Workshop model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                _context.Workshops.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Atölye veritabanına başarıyla eklendi.";
                return RedirectToAction("Workshops");
            }
            return View(model);
        }

        public IActionResult Fabricators()
        {
            var fabricators = _context.Fabricators.ToList();
            return View(fabricators);
        }

        public IActionResult CreateFabricator()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateFabricator(Fabricator model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                _context.Fabricators.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Kumaşçı veritabanına başarıyla eklendi.";
                return RedirectToAction("Fabricators");
            }
            return View(model);
        }

        public IActionResult Customers()
        {
            var customers = _context.Customers.ToList();
            return View(customers);
        }

        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCustomer(Customer model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                _context.Customers.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Müşteri veritabanına başarıyla eklendi.";
                return RedirectToAction("Customers");
            }
            return View(model);
        }
    }
}
