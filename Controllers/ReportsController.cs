using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using System.IO;

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

            // Varsayılan tarih filtresi (son 30 gün)
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            var query = _context.Orders.AsQueryable();
            var orders = query.Where(o => o.OrderDate >= start && o.OrderDate <= end).ToList();

            // Rapor istatistiklerini hesapla
            var model = new ReportsViewModel
            {
                TotalOrdersCount = orders.Count,
                TotalOrderQuantity = orders.Sum(o => o.Quantity),
                TotalAmount = orders.Sum(o => o.TotalAmount ?? 0),
                
                // Durum dağılımları
                StatusCounts = orders.GroupBy(o => (o.Status ?? "Yeni Kayıt")!)
                                     .ToDictionary(g => g.Key, g => g.Count()),
                                     
                // Atölye üretim adetleri
                WorkshopQuantities = orders.Where(o => !string.IsNullOrEmpty(o.SewingWorkshop))
                                           .GroupBy(o => o.SewingWorkshop!)
                                           .ToDictionary(g => g.Key, g => g.Sum(o => o.Quantity)),
                                           
                // Kumaş durum dağılımı
                FabricStatusCounts = orders.GroupBy(o => (o.FabricStatus ?? "Bekliyor")!)
                                           .ToDictionary(g => g.Key, g => g.Count()),

                // Zamanında teslimat oranları (Sevkiyat tamamlananlar arasından plan tarihlerine göre karşılaştırma)
                OnTimeCount = orders.Count(o => o.WarehouseArrivalDate.HasValue && o.DepartureDate.HasValue && 
                                               (!o.PlannedPackagingEndDate.HasValue || o.WarehouseArrivalDate <= o.PlannedPackagingEndDate)),
                DelayedCount = orders.Count(o => o.WarehouseArrivalDate.HasValue && o.PlannedPackagingEndDate.HasValue && 
                                                o.WarehouseArrivalDate > o.PlannedPackagingEndDate)
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

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var orders = _context.Orders
                                 .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sipariş Raporu");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "SİPARİŞ KODU";
                worksheet.Cell(currentRow, 2).Value = "MÜŞTERİ";
                worksheet.Cell(currentRow, 3).Value = "MODEL ADI";
                worksheet.Cell(currentRow, 4).Value = "SİPARİŞ TARİHİ";
                worksheet.Cell(currentRow, 5).Value = "MİKTAR (ADET)";
                worksheet.Cell(currentRow, 6).Value = "TUTAR";
                worksheet.Cell(currentRow, 7).Value = "ATÖLYE";
                worksheet.Cell(currentRow, 8).Value = "DURUM";

                var headerRange = worksheet.Range(1, 1, 1, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0047bb");
                headerRange.Style.Font.FontColor = XLColor.White;

                foreach (var order in orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.OrderCode;
                    worksheet.Cell(currentRow, 2).Value = order.Customer;
                    worksheet.Cell(currentRow, 3).Value = order.ModelName;
                    worksheet.Cell(currentRow, 4).Value = order.OrderDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 5).Value = order.Quantity;
                    worksheet.Cell(currentRow, 6).Value = order.TotalAmount ?? 0;
                    worksheet.Cell(currentRow, 7).Value = order.SewingWorkshop ?? "-";
                    worksheet.Cell(currentRow, 8).Value = order.Status ?? "Yeni Kayıt";
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Siparis_Raporu_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
                }
            }
        }
    }

    public class ReportsViewModel
    {
        public int TotalOrdersCount { get; set; }
        public int TotalOrderQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> WorkshopQuantities { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FabricStatusCounts { get; set; } = new Dictionary<string, int>();
        public int OnTimeCount { get; set; }
        public int DelayedCount { get; set; }
    }
}
