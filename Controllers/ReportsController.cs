using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate)
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            var orders = _context.Orders
                                 .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            var stokHareketleri = _context.StokHareketler
                                          .Include(sh => sh.StokKarti)
                                          .Where(sh => sh.OrderId != null && sh.HareketTipi == "Giriş")
                                          .ToList();

            var cariHareketler = _context.CariHareketler
                                         .Where(ch => ch.OrderId != null && ch.IslemTipi == "Alış Faturası")
                                         .ToList();

            var analysisList = new List<ActualAnalysisItem>();

            foreach (var o in orders)
            {
                var relatedStok = stokHareketleri.Where(sh => sh.OrderId == o.Id).ToList();
                var relatedCari = cariHareketler.Where(ch => ch.OrderId == o.Id).ToList();

                var actualFabric = relatedStok.Where(sh => sh.StokKarti != null && sh.StokKarti.Kategori == "Kumaş").Sum(sh => sh.Miktar);
                var actualAccessory = relatedStok.Where(sh => sh.StokKarti != null && sh.StokKarti.Kategori != "Kumaş").Sum(sh => sh.Miktar);
                
                var actualCost = relatedCari.Sum(ch => ch.Tutar);

                analysisList.Add(new ActualAnalysisItem
                {
                    OrderId = o.Id,
                    OrderCode = o.OrderCode,
                    ModelName = o.ModelName,
                    OrderDate = o.OrderDate,
                    OrderQuantity = o.Quantity,
                    PlannedFabric = (decimal)(o.PlannedFabricMeterage ?? o.FabricMeterage ?? 0),
                    ActualFabric = actualFabric,
                    ActualAccessory = actualAccessory,
                    ActualTotalCost = actualCost
                });
            }

            var model = new ActualAnalysisViewModel
            {
                TotalOrders = analysisList.Count,
                TotalPlannedFabric = analysisList.Sum(a => a.PlannedFabric),
                TotalActualFabric = analysisList.Sum(a => a.ActualFabric),
                TotalActualCost = analysisList.Sum(a => a.ActualTotalCost),
                Items = analysisList
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult ExportOrdersReport(DateTime? startDate, DateTime? endDate)
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Export implementation can be updated as needed
            // For now, redirecting to index.
            return RedirectToAction("Index");
        }
    }

    public class ActualAnalysisViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalPlannedFabric { get; set; }
        public decimal TotalActualFabric { get; set; }
        public decimal TotalActualCost { get; set; }
        public List<ActualAnalysisItem> Items { get; set; } = new List<ActualAnalysisItem>();
    }

    public class ActualAnalysisItem
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int OrderQuantity { get; set; }
        
        public decimal PlannedFabric { get; set; }
        public decimal ActualFabric { get; set; }
        public decimal FabricDiff => ActualFabric - PlannedFabric;
        public decimal FabricDiffPercentage => PlannedFabric > 0 ? (FabricDiff / PlannedFabric) * 100 : 0;
        
        public decimal ActualAccessory { get; set; }
        public decimal ActualTotalCost { get; set; }
    }
}
