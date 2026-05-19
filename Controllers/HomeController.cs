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

    public IActionResult Index()
    {
        var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
        var workshops = _context.Workshops.ToList();

        // 1. Planlanan Sipariş Sayısı
        ViewBag.PlannedOrdersCount = orders.Count;

        // 2. Aktif Atölye Sayısı
        var distinctActiveWorkshops = orders.Where(o => !string.IsNullOrEmpty(o.ProductionPlace)).Select(o => o.ProductionPlace).Distinct().Count();
        ViewBag.ActiveWorkshopsCount = distinctActiveWorkshops;
        ViewBag.TotalWorkshopsCount = workshops.Count;

        // 3. Çakışma & Eksik (Kumaş) Sayısı
        // Hedef kumaştan az teslim edilen siparişler veya Durumu "Bekleniyor" olanlar
        var missingFabricCount = orders.Count(o => o.FabricStatus == "Bekleniyor" || (o.TargetFabricQty.HasValue && o.ActualFabricQty.HasValue && o.ActualFabricQty.Value < o.TargetFabricQty.Value));
        ViewBag.MissingFabricCount = missingFabricCount;

        // 4. Ortalama Kumaş Karşılama Oranı (Kapasite Kullanımı yerine Kumaş Karşılama Performansı olarak gösterilebilir)
        decimal totalTargetFabric = orders.Sum(o => o.TargetFabricQty ?? 0);
        decimal totalActualFabric = orders.Sum(o => o.ActualFabricQty ?? 0);
        ViewBag.FabricMatchRate = totalTargetFabric > 0 ? Math.Round((totalActualFabric / totalTargetFabric) * 100, 1) : 100;

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
