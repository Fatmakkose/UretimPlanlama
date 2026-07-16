using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using ClosedXML.Excel;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UretimPlanlama.Controllers
{
    [Authorize(Policy = "SurecAccess")]
    public class WorkshopCapacityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkshopCapacityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var workshops = GetWorkshopData();
            return View(workshops);
        }

        public IActionResult Details(int id)
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var workshop = _context.Workshops.Find(id);
            if (workshop == null)
            {
                return NotFound();
            }

            var allOrders = _context.Orders.ToList();
            
            // Get all orders assigned to this workshop (including completed ones so they don't disappear)
            var activeOrders = allOrders
                .Where(o => o.Status != "İptal Edildi" &&
                            (o.SewingWorkshop == workshop.Name || 
                             o.ProductionPlace == workshop.Name || 
                             CheckWorkshopInJson(o.ProductionJson, workshop.Name)))
                .OrderBy(o => o.Status == "Tamamlandı" ? 1 : 0) // Aktifler üstte, tamamlananlar altta
                .ThenByDescending(o => o.OrderDate)
                .ToList();

            // Aktif olanları say (Sadece bilgi amaçlı, ama listeye hepsini yolluyoruz)
            var purelyActive = activeOrders.Where(o => o.Status != "Tamamlandı").ToList();

            var viewModel = new WorkshopDetailsViewModel
            {
                Workshop = workshop,
                ActiveOrders = activeOrders, // Listeye tamamlananlar da gidecek
                TotalActiveOrderCount = purelyActive.Count,
                TotalActivePieces = purelyActive.Sum(o => o.Quantity)
            };

            ViewBag.TimelineStatuses = activeOrders.ToDictionary(o => o.Id, o => GetTimelineStatus(o));

            return View(viewModel);
        }

        private bool CheckWorkshopInJson(string? json, string workshopName)
        {
            if (string.IsNullOrEmpty(json)) return false;
            try
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict == null) return false;
                
                if (dict.TryGetValue("prod_dikim_atolyesi", out var dikim) && dikim == workshopName) return true;
                if (dict.TryGetValue("prod_kesim_atolyesi", out var kesim) && kesim == workshopName) return true;
                if (dict.TryGetValue("prod_paketleme_atolyesi", out var paket) && paket == workshopName) return true;
            }
            catch {}
            return false;
        }

        private string GetTimelineStatus(Order order)
        {
            if (order.Status == "Tamamlandı" || order.Status == "İptal Edildi") return order.Status;

            var pDict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(order.ProductionJson)) { try { pDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.ProductionJson); } catch {} }
            
            var sDict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(order.SampleTestJson)) { try { sDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.SampleTestJson); } catch {} }

            string GetA(Dictionary<string,string> d, string k) => d != null && d.ContainsKey(k + "_actual") ? d[k + "_actual"] : "";

            if (!string.IsNullOrEmpty(GetA(pDict, "termin_tarihi"))) return "Sipariş Tamamlandı";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_depo_varis"))) return "Depo Varış";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_yola_cikis"))) return "Yola Çıkış";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_gs_gidisi"))) return "GS Gidişi";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_paket_bitis"))) return "Paket Bitiş";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_paket_baslangic"))) return "Paket Başlangıç";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_dikim_bitis"))) return "Dikim Bitiş";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_dikim_baslangic"))) return "Dikim Başlangıç";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_kesim_bitis"))) return "Kesim Bitiş";
            if (!string.IsNullOrEmpty(GetA(pDict, "prod_kesim_baslangic"))) return "Kesim Başlangıç";
            
            if (!string.IsNullOrEmpty(GetA(sDict, "sample_pp_onay"))) return "PP Onay";
            if (!string.IsNullOrEmpty(GetA(sDict, "sample_dugme_renk"))) return "Düğme Renk Kalite";
            if (!string.IsNullOrEmpty(GetA(sDict, "sample_tuse_renk"))) return "K-Tuşe Renk Onayı";
            if (!string.IsNullOrEmpty(GetA(sDict, "sample_kumas_ytesti"))) return "Kumaş Y-Testi Onayı";

            return order.Status;
        }

        [HttpPost]
        [HttpGet]
        public IActionResult ExportToExcel()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var workshops = GetWorkshopData();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Atölye Kapasite Raporu");
                var currentRow = 1;

                // Başlıklar
                worksheet.Cell(currentRow, 1).Value = "ATÖLYE ADI";
                worksheet.Cell(currentRow, 2).Value = "GÜNLÜK HEDEF";
                worksheet.Cell(currentRow, 3).Value = "GERÇEKLEŞEN";
                worksheet.Cell(currentRow, 4).Value = "DOLULUK ORANI";
                worksheet.Cell(currentRow, 5).Value = "DURUM";

                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0047bb");
                headerRange.Style.Font.FontColor = XLColor.White;

                // Veri Satırları
                foreach (var ws in workshops)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = ws.Name;
                    worksheet.Cell(currentRow, 2).Value = ws.DailyTarget;
                    worksheet.Cell(currentRow, 3).Value = ws.ActualProduction;
                    worksheet.Cell(currentRow, 4).Value = $"{(ws.CapacityUsage * 100):N1}%";
                    worksheet.Cell(currentRow, 5).Value = ws.Status;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CPS_Atolye_Kapasite_Raporu.xlsx");
                }
            }
        }

        private List<WorkshopCapacityViewModel> GetWorkshopData()
        {
            var activeOrders = _context.Orders.Where(o => o.Status != "Tamamlandı" && o.Status != "İptal Edildi").ToList();
            var activeWorkshopNames = new HashSet<string>();
            foreach(var o in activeOrders)
            {
                if (!string.IsNullOrEmpty(o.SewingWorkshop)) activeWorkshopNames.Add(o.SewingWorkshop);
                if (!string.IsNullOrEmpty(o.ProductionPlace)) activeWorkshopNames.Add(o.ProductionPlace);
                if (!string.IsNullOrEmpty(o.ProductionJson))
                {
                    try {
                        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(o.ProductionJson);
                        if (dict != null)
                        {
                            if (dict.TryGetValue("prod_dikim_atolyesi", out var d) && !string.IsNullOrEmpty(d)) activeWorkshopNames.Add(d);
                            if (dict.TryGetValue("prod_kesim_atolyesi", out var k) && !string.IsNullOrEmpty(k)) activeWorkshopNames.Add(k);
                            if (dict.TryGetValue("prod_paketleme_atolyesi", out var p) && !string.IsNullOrEmpty(p)) activeWorkshopNames.Add(p);
                        }
                    } catch {}
                }
            }

            var dbWorkshops = _context.Workshops.ToList();
            var result = new List<WorkshopCapacityViewModel>();

            int idCounter = 1;
            foreach (var wName in activeWorkshopNames.OrderBy(n => n))
            {
                var dbW = dbWorkshops.FirstOrDefault(w => w.Name == wName);
                int dailyTarget = dbW != null && dbW.DailyCapacity > 0 ? dbW.DailyCapacity : 3000;
                string wType = dbW != null && !string.IsNullOrEmpty(dbW.Type) ? dbW.Type : "Dikim";
                int wId = dbW != null ? dbW.Id : idCounter++;

                int assignedWork = activeOrders.Where(o => 
                    o.SewingWorkshop == wName || 
                    o.ProductionPlace == wName || 
                    CheckWorkshopInJson(o.ProductionJson, wName)
                ).Sum(o => o.CalculatedQuantity);

                // Haftalık kapasiteye oranlayalım (5 gün)
                double weeklyCapacity = dailyTarget * 5.0;
                double capacityUsage = weeklyCapacity > 0 ? (double)assignedWork / weeklyCapacity : 0;
                if (capacityUsage > 1.0) capacityUsage = 1.0;

                string status = "Çalışıyor";
                if (capacityUsage >= 0.9) status = "Tam Dolu";
                else if (capacityUsage >= 0.5) status = "Yoğun";
                else if (capacityUsage > 0) status = "Çalışıyor";
                else status = "Müsait";

                result.Add(new WorkshopCapacityViewModel
                {
                    Id = wId,
                    Name = wName,
                    DailyTarget = dailyTarget,
                    ActualProduction = assignedWork, // Aktif İş Yükü olarak kullanıyoruz
                    CapacityUsage = capacityUsage,
                    Status = status,
                    DailyData = GenerateDailyData(wName, wType, dailyTarget)
                });
            }

            return result;
        }

        private List<WorkshopDailyData> GenerateDailyData(string workshopName, string workshopType, int dailyTarget)
        {
            var dailyDataList = new List<WorkshopDailyData>();
            string[] days = { "Pzt", "Sal", "Çar", "Per", "Cum" };

            // Son gerçek üretim tarihini bul, eğer yoksa bugünü kullan (LINQ çeviri hatasını önlemek için belleğe çekilir)
            var allEndDates = _context.Orders
                .Select(o => new { o.SewingEndDate, o.CuttingEndDate, o.PackagingEndDate })
                .ToList();

            var latestActualDate = allEndDates
                .SelectMany(o => new[] { o.SewingEndDate, o.CuttingEndDate, o.PackagingEndDate })
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .DefaultIfEmpty(DateTime.Today)
                .Max();

            // Bu tarihin ait olduğu haftanın pazartesi gününü bul
            int diff = (7 + (latestActualDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = latestActualDate.AddDays(-1 * diff).Date;

            var orders = _context.Orders.ToList();

            for (int d = 0; d < 5; d++)
            {
                var targetDate = monday.AddDays(d);
                int actual = 0;

                // Atölye tipine göre o gün tamamlanan üretim miktarını hesapla
                if (workshopType.Contains("Kesim", StringComparison.OrdinalIgnoreCase))
                {
                    actual = orders
                        .Where(o => o.CuttingEndDate.HasValue && 
                                    (o.CuttingEndDate.Value.Date == targetDate || 
                                     (d == 4 && (o.CuttingEndDate.Value.Date == targetDate.AddDays(1) || o.CuttingEndDate.Value.Date == targetDate.AddDays(2)))) &&
                                    (o.SewingWorkshop == workshopName || o.ProductionPlace == workshopName || CheckWorkshopInJson(o.ProductionJson, workshopName)))
                        .Sum(o => o.CalculatedQuantity);
                }
                else if (workshopType.Contains("Paket", StringComparison.OrdinalIgnoreCase) || 
                         workshopType.Contains("Lojistik", StringComparison.OrdinalIgnoreCase))
                {
                    actual = orders
                        .Where(o => o.PackagingEndDate.HasValue && 
                                    (o.PackagingEndDate.Value.Date == targetDate || 
                                     (d == 4 && (o.PackagingEndDate.Value.Date == targetDate.AddDays(1) || o.PackagingEndDate.Value.Date == targetDate.AddDays(2)))) &&
                                    (o.SewingWorkshop == workshopName || o.ProductionPlace == workshopName || CheckWorkshopInJson(o.ProductionJson, workshopName)))
                        .Sum(o => o.CalculatedQuantity);
                }
                else // Dikim veya Diğer
                {
                    actual = orders
                        .Where(o => o.SewingEndDate.HasValue && 
                                    (o.SewingEndDate.Value.Date == targetDate || 
                                     (d == 4 && (o.SewingEndDate.Value.Date == targetDate.AddDays(1) || o.SewingEndDate.Value.Date == targetDate.AddDays(2)))) &&
                                    (o.SewingWorkshop == workshopName || o.ProductionPlace == workshopName || CheckWorkshopInJson(o.ProductionJson, workshopName)))
                        .Sum(o => o.CalculatedQuantity);
                }

                string status = "Çalışıyor";
                if (actual == 0)
                {
                    status = "Müsait";
                }
                else if (actual > dailyTarget)
                {
                    status = "Kapasite Aşımı";
                }

                double usage = dailyTarget > 0 ? (double)actual / dailyTarget : 0;
                dailyDataList.Add(new WorkshopDailyData
                {
                    DayName = days[d],
                    ActualProduction = actual,
                    CapacityUsage = usage,
                    Status = status
                });
            }
            return dailyDataList;
        }
    }

    public class WorkshopCapacityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DailyTarget { get; set; }
        public int ActualProduction { get; set; }
        public double CapacityUsage { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<WorkshopDailyData> DailyData { get; set; } = new List<WorkshopDailyData>();
    }

    public class WorkshopDailyData
    {
        public string DayName { get; set; } = string.Empty;
        public int ActualProduction { get; set; }
        public double CapacityUsage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class WorkshopDetailsViewModel
    {
        public Workshop Workshop { get; set; } = null!;
        public List<Order> ActiveOrders { get; set; } = new List<Order>();
        public int TotalActiveOrderCount { get; set; }
        public int TotalActivePieces { get; set; }
    }
}
