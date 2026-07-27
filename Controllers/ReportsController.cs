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
    [Authorize(Policy = "RaporAccess")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
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
