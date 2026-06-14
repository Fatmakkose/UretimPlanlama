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
    [Authorize]
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
            var dbWorkshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            var result = new List<WorkshopCapacityViewModel>();

            if (dbWorkshops.Any())
            {
                foreach (var w in dbWorkshops)
                {
                    int dailyTarget = w.DailyCapacity > 0 ? w.DailyCapacity : 3000;
                    var dailyData = GenerateDailyData(w.Name, w.Type, dailyTarget);
                    var defaultDay = dailyData[2]; // Görsel taslakla (mock-up) eşleşmesi için varsayılan olarak Çarşamba ("Çar") seçilir

                    result.Add(new WorkshopCapacityViewModel
                    {
                        Id = w.Id,
                        Name = w.Name,
                        DailyTarget = dailyTarget,
                        ActualProduction = defaultDay.ActualProduction,
                        CapacityUsage = defaultDay.CapacityUsage,
                        Status = defaultDay.Status,
                        DailyData = dailyData
                    });
                }
            }
            else
            {
                // Veritabanı boşsa görsel taslaktan (mock-up) gelen yedek / simüle veriler
                var ws1 = new WorkshopCapacityViewModel { Id = 1, Name = "Ana Kesim - A1", DailyTarget = 4500 };
                ws1.DailyData = GenerateDailyData(ws1.Name, "Kesim", ws1.DailyTarget);
                var defaultDay1 = ws1.DailyData[2];
                ws1.ActualProduction = defaultDay1.ActualProduction;
                ws1.CapacityUsage = defaultDay1.CapacityUsage;
                ws1.Status = defaultDay1.Status;
                result.Add(ws1);

                var ws2 = new WorkshopCapacityViewModel { Id = 2, Name = "Dikim Hattı - B4", DailyTarget = 3200 };
                ws2.DailyData = GenerateDailyData(ws2.Name, "Dikim", ws2.DailyTarget);
                var defaultDay2 = ws2.DailyData[2];
                ws2.ActualProduction = defaultDay2.ActualProduction;
                ws2.CapacityUsage = defaultDay2.CapacityUsage;
                ws2.Status = defaultDay2.Status;
                result.Add(ws2);

                var ws3 = new WorkshopCapacityViewModel { Id = 3, Name = "Kalite Kontrol", DailyTarget = 5000 };
                ws3.DailyData = GenerateDailyData(ws3.Name, "Paketleme", ws3.DailyTarget);
                var defaultDay3 = ws3.DailyData[2];
                ws3.ActualProduction = defaultDay3.ActualProduction;
                ws3.CapacityUsage = defaultDay3.CapacityUsage;
                ws3.Status = defaultDay3.Status;
                result.Add(ws3);

                var ws4 = new WorkshopCapacityViewModel { Id = 4, Name = "Paketleme Ünitesi", DailyTarget = 4000 };
                ws4.DailyData = GenerateDailyData(ws4.Name, "Paketleme", ws4.DailyTarget);
                var defaultDay4 = ws4.DailyData[2];
                ws4.ActualProduction = defaultDay4.ActualProduction;
                ws4.CapacityUsage = defaultDay4.CapacityUsage;
                ws4.Status = defaultDay4.Status;
                result.Add(ws4);

                var ws5 = new WorkshopCapacityViewModel { Id = 5, Name = "Lojistik Hazırlık", DailyTarget = 2500 };
                ws5.DailyData = GenerateDailyData(ws5.Name, "Lojistik", ws5.DailyTarget);
                var defaultDay5 = ws5.DailyData[2];
                ws5.ActualProduction = defaultDay5.ActualProduction;
                ws5.CapacityUsage = defaultDay5.CapacityUsage;
                ws5.Status = defaultDay5.Status;
                result.Add(ws5);
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
                                    (o.SewingWorkshop == workshopName || o.ProductionPlace == workshopName))
                        .Sum(o => o.Quantity);
                }
                else if (workshopType.Contains("Paket", StringComparison.OrdinalIgnoreCase) || 
                         workshopType.Contains("Lojistik", StringComparison.OrdinalIgnoreCase))
                {
                    actual = orders
                        .Where(o => o.PackagingEndDate.HasValue && 
                                    (o.PackagingEndDate.Value.Date == targetDate || 
                                     (d == 4 && (o.PackagingEndDate.Value.Date == targetDate.AddDays(1) || o.PackagingEndDate.Value.Date == targetDate.AddDays(2)))) &&
                                    (o.SewingWorkshop == workshopName || o.ProductionPlace == workshopName))
                        .Sum(o => o.Quantity);
                }
                else // Dikim veya Diğer
                {
                    actual = orders
                        .Where(o => o.SewingEndDate.HasValue && 
                                    (o.SewingEndDate.Value.Date == targetDate || 
                                     (d == 4 && (o.SewingEndDate.Value.Date == targetDate.AddDays(1) || o.SewingEndDate.Value.Date == targetDate.AddDays(2)))) &&
                                    (o.SewingWorkshop == workshopName || o.ProductionPlace == workshopName))
                        .Sum(o => o.Quantity);
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
}
