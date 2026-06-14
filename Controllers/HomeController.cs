using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly Data.ApplicationDbContext _context;

    public HomeController(Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public class WorkshopFabricSummary
    {
        public string? WorkshopName { get; set; }
        public decimal TotalTarget { get; set; }
        public decimal TotalActual { get; set; }
        public decimal MatchRate => TotalTarget > 0 ? (TotalActual / TotalTarget) * 100 : 100;
    }

    public class WorkshopCapacityStatus
    {
        public Workshop Workshop { get; set; } = null!;
        public int DailyUsage { get; set; }
        public int MonthlyUsage { get; set; }
        public int AnnualUsage { get; set; }
        public double DailyOccupancyRate { get; set; }
        public double MonthlyOccupancyRate { get; set; }
        public double AnnualOccupancyRate { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }

    public IActionResult Index()
    {
        var orders = _context.Orders.Where(o => o.ModelName != "Test Model").OrderByDescending(o => o.OrderDate).ToList();
        var workshops = _context.Workshops.ToList();

        // Yeni Dashboard İstatistikleri
        ViewBag.TotalOrdersQty = orders.Sum(o => o.Quantity);
        ViewBag.CuttingQty = orders.Where(o => o.Status == "Kesim" || (o.CuttingStartDate != null && o.CuttingEndDate == null)).Sum(o => o.Quantity);
        ViewBag.SewingQty = orders.Where(o => o.Status == "Dikim" || (o.SewingStartDate != null && o.SewingEndDate == null)).Sum(o => o.Quantity);
        ViewBag.ReadyToShipQty = orders.Where(o => o.Status == "Paket" || o.Status == "Sevkiyata Hazır" || (o.PackagingStartDate != null)).Sum(o => o.Quantity);


        // Atölye bazlı Kumaş Karşılaştırma Takibi
        var workshopSummaries = orders
            .Where(o => !string.IsNullOrEmpty(o.ProductionPlace))
            .GroupBy(o => o.ProductionPlace)
            .Select(g => new WorkshopFabricSummary
            {
                WorkshopName = g.Key,
                TotalTarget = g.Sum(o => o.TargetFabricQty ?? 0),
                TotalActual = g.Sum(o => o.ActualFabricQty ?? 0)
            })
            .ToList();

        ViewBag.WorkshopSummaries = workshopSummaries;

        // Atölye bazlı Kapasite ve Doluluk Takibi
        var today = DateTime.Today;
        var currentMonth = today.Month;
        var currentYear = today.Year;

        var capacityStatuses = new List<WorkshopCapacityStatus>();
        foreach (var w in workshops)
        {
            var wOrders = orders
                .Where(o => (o.SewingWorkshop == w.Name || o.ProductionPlace == w.Name) && o.Status != "İptal Edildi")
                .ToList();

            var dailyUsage = wOrders.Where(o => o.OrderDate.Date == today).Sum(o => o.Quantity);
            var monthlyUsage = wOrders.Where(o => o.OrderDate.Year == currentYear && o.OrderDate.Month == currentMonth).Sum(o => o.Quantity);
            var annualUsage = wOrders.Where(o => o.OrderDate.Year == currentYear).Sum(o => o.Quantity);

            var dailyRate = w.DailyCapacity > 0 ? ((double)dailyUsage / w.DailyCapacity) * 100 : 0;
            var monthlyRate = w.MonthlyCapacity > 0 ? ((double)monthlyUsage / w.MonthlyCapacity) * 100 : 0;
            var annualRate = w.AnnualCapacity > 0 ? ((double)annualUsage / w.AnnualCapacity) * 100 : 0;

            // En kritik doluluk oranına göre durum belirle
            var primaryRate = w.MonthlyCapacity > 0 ? monthlyRate : (w.DailyCapacity > 0 ? dailyRate : 0);
            
            string statusLabel = "Boş / Müsait";
            string statusClass = "badge-progress"; // Yeşil
            
            if (primaryRate >= 100)
            {
                statusLabel = "Kapasite Dolu";
                statusClass = "badge-high"; // Kırmızı
            }
            else if (primaryRate >= 75)
            {
                statusLabel = "Yoğun Çalışıyor";
                statusClass = "badge-medium"; // Sarı
            }

            capacityStatuses.Add(new WorkshopCapacityStatus
            {
                Workshop = w,
                DailyUsage = dailyUsage,
                MonthlyUsage = monthlyUsage,
                AnnualUsage = annualUsage,
                DailyOccupancyRate = Math.Round(dailyRate, 1),
                MonthlyOccupancyRate = Math.Round(monthlyRate, 1),
                AnnualOccupancyRate = Math.Round(annualRate, 1),
                StatusLabel = statusLabel,
                StatusClass = statusClass
            });
        }

        ViewBag.WorkshopCapacities = capacityStatuses;

        return View(orders);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
