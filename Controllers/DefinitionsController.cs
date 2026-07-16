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

        [HttpGet]
        public IActionResult GetWorkshopDetail(int id)
        {
            var workshop = _context.Workshops.Find(id);
            if (workshop == null)
            {
                return Json(new { success = false, message = "Atölye bulunamadı." });
            }
            var orders = _context.Orders
                .Where(o => o.SewingWorkshop == workshop.Name || (o.ProductionJson != null && o.ProductionJson.Contains(workshop.Name)))
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return Json(new { success = true, workshop = workshop, orders = orders });
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

        public IActionResult EditWorkshop(int id)
        {
            var workshop = _context.Workshops.Find(id);
            if (workshop == null)
            {
                return NotFound();
            }
            return View(workshop);
        }

        [HttpPost]
        public IActionResult EditWorkshop(Workshop model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                var ws = _context.Workshops.Find(model.Id);
                if (ws != null)
                {
                    ws.Name = model.Name;
                    ws.Type = model.Type;
                    ws.AuthorizedPerson = model.AuthorizedPerson;
                    ws.DailyCapacity = model.DailyCapacity;
                    ws.MonthlyCapacity = model.MonthlyCapacity;
                    ws.AnnualCapacity = model.AnnualCapacity;
                    ws.Address = model.Address;

                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Atölye kapasite ve bilgileri başarıyla güncellendi.";
                    return RedirectToAction("Workshops");
                }
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

        // --- FIRMA (COMPANY) TANIMLARI ---
        public IActionResult Companies()
        {
            var companies = _context.Companies.ToList();
            return View(companies);
        }

        public IActionResult CreateCompany()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCompany(Company model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                _context.Companies.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Firma başarıyla veritabanına eklendi.";
                return RedirectToAction("Companies");
            }
            return View(model);
        }

        // --- AKSESUAR (ACCESSORY) TANIMLARI ---
        public IActionResult Accessories()
        {
            var accessories = _context.Accessories.ToList();
            return View(accessories);
        }

        public IActionResult CreateAccessory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAccessory(Accessory model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                _context.Accessories.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Aksesuar başarıyla veritabanına eklendi.";
                return RedirectToAction("Accessories");
            }
            return View(model);
        }
        // --- MARKA (BRAND) TANIMLARI ---
        public IActionResult Brands()
        {
            var brands = _context.Brands.ToList();
            return View(brands);
        }

        public IActionResult CreateBrand()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateBrand(Brand model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                _context.Brands.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Marka başarıyla veritabanına eklendi.";
                return RedirectToAction("Brands");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ToggleWorkshopStatus(int id)
        {
            var workshop = _context.Workshops.Find(id);
            if (workshop != null)
            {
                workshop.IsActive = !workshop.IsActive;
                _context.SaveChanges();
                return Json(new { success = true, isActive = workshop.IsActive });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult ToggleFabricatorStatus(int id)
        {
            var fabricator = _context.Fabricators.Find(id);
            if (fabricator != null)
            {
                fabricator.IsActive = !fabricator.IsActive;
                _context.SaveChanges();
                return Json(new { success = true, isActive = fabricator.IsActive });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public IActionResult DeleteFabricator(int id)
        {
            var fabricator = _context.Fabricators.Find(id);
            if (fabricator != null)
            {
                _context.Fabricators.Remove(fabricator);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Kumaşçı bulunamadı." });
        }
    }
}
