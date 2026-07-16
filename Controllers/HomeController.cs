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
        public int ActiveOrderCount { get; set; }
    }

    public IActionResult Index()
    {
        var orders = _context.Orders.Where(o => o.ModelName != "Test Model").OrderByDescending(o => o.OrderDate).ToList();
        var workshops = _context.Workshops.ToList();

        // Yeni Dashboard İstatistikleri ve Aşama Dağılımı
        int waitingQty = 0;
        int cuttingQty = 0;
        int completedQty = 0;

        int stageYeniKayit = 0;
        int stageKesim = 0;
        int stageDikim = 0;
        int stagePaket = 0;

        foreach (var o in orders)
        {
            var pDict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(o.ProductionJson)) { try { pDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(o.ProductionJson); } catch {} }

            bool HasP(string key) => pDict != null && pDict.ContainsKey(key) && !string.IsNullOrEmpty(pDict[key]);

            bool isPaketBitis = o.PackagingEndDate.HasValue || HasP("prod_paket_bitis_actual");
            bool isPaket = o.PackagingStartDate.HasValue || HasP("prod_paket_baslangic_actual") || isPaketBitis;
            bool isDikimBitis = o.SewingEndDate.HasValue || HasP("prod_dikim_bitis_actual");
            bool isDikim = o.SewingStartDate.HasValue || HasP("prod_dikim_baslangic_actual") || isDikimBitis || isPaket;
            bool isKesimBitis = o.CuttingEndDate.HasValue || HasP("prod_kesim_bitis_actual") || isDikim;
            bool isKesim = o.CuttingStartDate.HasValue || HasP("prod_kesim_baslangic_actual") || isKesimBitis;

            // Kesim KPI Hesaplamaları
            if (isKesimBitis) {
                completedQty += o.CalculatedQuantity;
            } else if (isKesim) {
                cuttingQty += o.CalculatedQuantity;
            } else {
                waitingQty += o.CalculatedQuantity;
            }

            // Pasta Grafik Dağılımı
            if (isPaket) {
                stagePaket++;
            } else if (isDikim) {
                stageDikim++;
            } else if (isKesim) {
                stageKesim++;
            } else {
                stageYeniKayit++;
            }
        }

        ViewBag.TotalOrdersQty = orders.Sum(o => o.CalculatedQuantity);
        ViewBag.WaitingQty = waitingQty;
        ViewBag.CuttingQty = cuttingQty;
        ViewBag.CompletedQty = completedQty;

        // Kritik Siparişler (Gecikenler)
        var criticalOrders = orders.Where(o => o.EffectiveTerminDate < DateTime.Today && o.Status != "Tamamlandı" && o.Status != "İptal Edildi").OrderBy(o => o.EffectiveTerminDate).ToList();
        ViewBag.CriticalOrders = criticalOrders;

        // Yaklaşan Terminler (Önümüzdeki 15 gün)
        var upcomingDeadlines = orders.Where(o => o.EffectiveTerminDate >= DateTime.Today && o.EffectiveTerminDate <= DateTime.Today.AddDays(15) && o.Status != "Tamamlandı" && o.Status != "İptal Edildi").OrderBy(o => o.EffectiveTerminDate).ToList();
        ViewBag.UpcomingDeadlines = upcomingDeadlines;

        ViewBag.StageDistribution = new[] { stageYeniKayit, stageKesim, stageDikim, stagePaket };

        // Son Hareketler (Bildirimler)
        ViewBag.RecentNotifications = _context.Notifications.OrderByDescending(n => n.CreatedAt).Take(8).ToList();
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
                .Where(o => {
                    if (o.Status == "İptal Edildi") return false;
                    if (o.SewingWorkshop == w.Name || o.ProductionPlace == w.Name) return true;
                    if (!string.IsNullOrEmpty(o.ProductionJson)) {
                        try {
                            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(o.ProductionJson);
                            if (dict != null && dict.TryGetValue("prod_dikim_atolyesi", out var dikim) && dikim == w.Name) return true;
                        } catch {}
                    }
                    return false;
                })
                .ToList();

            int dailyUsage = 0;
            int monthlyUsage = 0;
            int annualUsage = 0;

            foreach (var o in wOrders)
            {
                DateTime refDate = o.SewingStartDate ?? o.PlannedSewingStartDate ?? o.OrderDate;
                int qty = o.CalculatedQuantity > 0 ? o.CalculatedQuantity : o.Quantity;
                
                if (!string.IsNullOrEmpty(o.ProductionJson) && o.ProductionJson.Contains("\"prod_dikim_miktari\""))
                {
                    try {
                        var pDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(o.ProductionJson);
                        if (pDict != null && pDict.ContainsKey("prod_dikim_miktari") && int.TryParse(pDict["prod_dikim_miktari"], out int pdm)) {
                            qty = pdm;
                        }
                    } catch {}
                }

                if (refDate.Date == today) dailyUsage += qty;
                if (refDate.Year == currentYear && refDate.Month == currentMonth) monthlyUsage += qty;
                if (refDate.Year == currentYear) annualUsage += qty;
            }

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
                StatusClass = statusClass,
                ActiveOrderCount = wOrders.Count
            });
        }
        // Sadece üzerinde siparişi olan atölyeleri göster
        ViewBag.WorkshopCapacities = capacityStatuses
            .Where(c => c.ActiveOrderCount > 0)
            .OrderByDescending(c => c.MonthlyOccupancyRate)
            .ToList();
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> ClearNotifications()
    {
        try
        {
            _context.Notifications.RemoveRange(_context.Notifications);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (System.Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
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
