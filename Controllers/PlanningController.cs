using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using ClosedXML.Excel;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using UretimPlanlama.Hubs;
using System.Collections.Generic;
using System.Linq;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class PlanningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PlanningController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.AllOrders = orders;
            return View("Plan", new Order());
        }

        [HttpGet]
        public IActionResult Plan(int id)
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            var order = orders.FirstOrDefault(o => o.Id == id);
            
            if (order == null) return NotFound();

            ViewBag.AllOrders = orders;
            return View(order);
        }

        [HttpPost]
        public IActionResult UpdatePlan(Order orderData)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var order = _context.Orders.Find(orderData.Id);
            if (order != null)
            {
                order.FabricSupplier = orderData.FabricSupplier;
                order.FabricArrivalAgreedDate = orderData.FabricArrivalAgreedDate;
                
                order.PlannedCuttingStartDate = orderData.PlannedCuttingStartDate;
                order.PlannedCuttingEndDate = orderData.PlannedCuttingEndDate;

                order.SewingWorkshop = orderData.SewingWorkshop;
                order.PlannedSewingStartDate = orderData.PlannedSewingStartDate;
                order.PlannedSewingEndDate = orderData.PlannedSewingEndDate;

                order.PlannedPackagingStartDate = orderData.PlannedPackagingStartDate;
                order.PlannedPackagingEndDate = orderData.PlannedPackagingEndDate;
                order.PlannedLastInspectionDate = orderData.PlannedLastInspectionDate;

                order.UnitCost = orderData.UnitCost;
                order.UnitPrice = orderData.UnitPrice;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Planlama detayları başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult UpdatePurchasingPlan(Order orderData, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var order = _context.Orders.Find(orderData.Id);
            if (order != null)
            {
                order.ActualFabricMeterage = orderData.ActualFabricMeterage;
                order.ActualFabricQty = orderData.ActualFabricQty;
                
                order.FabricSupplier = orderData.FabricSupplier;
                order.FabricArrivalAgreedDate = orderData.FabricArrivalAgreedDate;
                order.FabricArrivalActualDate = orderData.FabricArrivalActualDate;

                // Extra fields in PurchasingMaterialsJson
                var dict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(orderData.PurchasingMaterialsJson))
                {
                    try {
                        dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(orderData.PurchasingMaterialsJson) ?? new Dictionary<string, string>();
                    } catch {}
                }

                foreach (var k in form.Keys) {
                    if (k.StartsWith("pur_")) {
                        dict[k] = form[k].ToString();
                    }
                }
                
                order.PurchasingMaterialsJson = System.Text.Json.JsonSerializer.Serialize(dict);
                
                // Satın Alma Tamamlandı mı?
                order.IsPurchasingCompleted = orderData.IsPurchasingCompleted;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Satın Alma planı güncellendi.";
                return RedirectToAction("Plan", new { id = order.Id });
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult UpdateSampleTestPlan(int Id, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var order = _context.Orders.Find(Id);
            if (order != null)
            {
                var dict = new Dictionary<string, string>();
                
                string[] keys = new[] { "sample_pp_onay", "sample_kumas_ytesti", "sample_tuse_renk", "sample_kumas_karisim", "sample_dugme_renk", "sample_dugme_test" };
                
                foreach (var k in keys) {
                    if (form.ContainsKey(k)) {
                        dict[k] = form[k].ToString();
                    }
                }

                order.SampleTestJson = System.Text.Json.JsonSerializer.Serialize(dict);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Numune ve Test planı güncellendi.";
                return RedirectToAction("Plan", new { id = order.Id });
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult UpdateProductionPlan(int Id, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var order = _context.Orders.Find(Id);
            if (order != null)
            {
                var dict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.ProductionJson))
                {
                    try {
                        dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.ProductionJson) ?? new Dictionary<string, string>();
                    } catch {}
                }

                foreach (var k in form.Keys) {
                    if (k.StartsWith("prod_")) {
                        dict[k] = form[k].ToString();
                    }
                }

                order.ProductionJson = System.Text.Json.JsonSerializer.Serialize(dict);
                
                if (form.ContainsKey("IsProductionCompleted")) {
                    order.IsProductionCompleted = form["IsProductionCompleted"] == "true";
                } else {
                    order.IsProductionCompleted = false;
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Üretim planı güncellendi.";
                return RedirectToAction("Plan", new { id = order.Id });
            }
            return NotFound();
        }
        [HttpGet]
        public IActionResult Tracking()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateTracking(Order orderData)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var old = _context.Orders.AsNoTracking().FirstOrDefault(o => o.Id == orderData.Id);
            var order = _context.Orders.Find(orderData.Id);
            if (order != null)
            {
                order.FabricArrivalActualDate = orderData.FabricArrivalActualDate;
                order.FabricMeterage = orderData.FabricMeterage;
                order.ActualFabricQty = orderData.ActualFabricQty;

                order.CuttingStartDate = orderData.CuttingStartDate;
                order.CuttingEndDate = orderData.CuttingEndDate;

                order.SewingStartDate = orderData.SewingStartDate;
                order.SewingEndDate = orderData.SewingEndDate;

                order.PackagingStartDate = orderData.PackagingStartDate;
                order.PackagingEndDate = orderData.PackagingEndDate;
                order.LastInspectionDate = orderData.LastInspectionDate;

                order.DepartureDate = orderData.DepartureDate;
                order.WarehouseArrivalDate = orderData.WarehouseArrivalDate;

                _context.SaveChanges();

                if (old != null)
                {
                    var notifications = new List<Notification>();

                    void CheckDate(DateTime? oldDate, DateTime? newDate, string title, string messageTemplate, string type)
                    {
                        if (newDate.HasValue && (!oldDate.HasValue || oldDate.Value.Date != newDate.Value.Date))
                        {
                            notifications.Add(new Notification
                            {
                                Title = title,
                                Message = string.Format(messageTemplate, order.OrderCode, newDate.Value.ToString("dd.MM.yyyy")),
                                Type = type,
                                OrderCode = order.OrderCode,
                                CreatedAt = DateTime.Now,
                                IsRead = false
                            });
                        }
                    }

                    CheckDate(old.FabricArrivalActualDate, orderData.FabricArrivalActualDate, "Kumaş Ulaştı", "{0} nolu sipariş için kumaş geliş tarihi {1} olarak güncellendi.", "Kumaş");
                    CheckDate(old.CuttingStartDate, orderData.CuttingStartDate, "Kesim Başladı", "{0} nolu sipariş için kesim başlangıcı {1} olarak girildi.", "Kesim");
                    CheckDate(old.CuttingEndDate, orderData.CuttingEndDate, "Kesim Bitti", "{0} nolu sipariş için kesim bitişi {1} olarak girildi.", "Kesim");
                    CheckDate(old.SewingStartDate, orderData.SewingStartDate, "Dikim Başladı", "{0} nolu sipariş için dikim başlangıcı {1} olarak girildi.", "Dikim");
                    CheckDate(old.SewingEndDate, orderData.SewingEndDate, "Dikim Bitti", "{0} nolu sipariş için dikim bitişi {1} olarak girildi.", "Dikim");
                    CheckDate(old.PackagingStartDate, orderData.PackagingStartDate, "Paketleme Başladı", "{0} nolu sipariş için paketleme başlangıcı {1} olarak girildi.", "Paket");
                    CheckDate(old.PackagingEndDate, orderData.PackagingEndDate, "Paketleme Bitti", "{0} nolu sipariş için paketleme bitişi {1} olarak girildi.", "Paket");

                    if (notifications.Any())
                    {
                        _context.Notifications.AddRange(notifications);
                        _context.SaveChanges();

                        foreach (var notif in notifications)
                        {
                            _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                            {
                                id = notif.Id,
                                title = notif.Title,
                                message = notif.Message,
                                type = notif.Type,
                                createdAt = notif.CreatedAt.ToString("HH:mm - dd.MM.yyyy"),
                                isRead = notif.IsRead
                            });
                        }
                    }
                }

                TempData["SuccessMessage"] = "Takip detayları başarıyla kaydedildi.";
                return RedirectToAction(nameof(Tracking));
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Planlama Takip");
                var currentRow = 1;

                // Başlıklar
                worksheet.Cell(currentRow, 1).Value = "MÜŞTERİ";
                worksheet.Cell(currentRow, 2).Value = "MODEL ADI";
                worksheet.Cell(currentRow, 3).Value = "RENK";
                worksheet.Cell(currentRow, 4).Value = "ÇLŞ";
                worksheet.Cell(currentRow, 5).Value = "PO TARİHİ";
                worksheet.Cell(currentRow, 6).Value = "SİPARİŞ KODU";
                worksheet.Cell(currentRow, 7).Value = "SİP ADETİ";
                worksheet.Cell(currentRow, 8).Value = "MODEL DETAY";
                worksheet.Cell(currentRow, 9).Value = "KUMAŞÇI";
                worksheet.Cell(currentRow, 10).Value = "KUMAŞ SEVK-ANLAŞILAN";
                worksheet.Cell(currentRow, 11).Value = "KUMAŞ GELİŞ TARİHİ";
                worksheet.Cell(currentRow, 12).Value = "KESİM BAŞLANGIÇ";
                worksheet.Cell(currentRow, 13).Value = "KESİM BİTİŞ";
                worksheet.Cell(currentRow, 14).Value = "DİKİM BAŞLANGIÇ";
                worksheet.Cell(currentRow, 15).Value = "DİKİM BİTİŞ";
                worksheet.Cell(currentRow, 16).Value = "PAKET BAŞLANGIÇ";
                worksheet.Cell(currentRow, 17).Value = "GS GİDİŞİ";
                worksheet.Cell(currentRow, 18).Value = "PAKET BİTİŞ";
                worksheet.Cell(currentRow, 19).Value = "YOLA ÇIKIŞ";
                worksheet.Cell(currentRow, 20).Value = "DEPO VARIŞ";
                worksheet.Cell(currentRow, 21).Value = "SON INSPC TARİHİ";
                worksheet.Cell(currentRow, 22).Value = "DİKİM ATÖLYESİ";

                var headerRange = worksheet.Range(1, 1, 1, 22);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Veriler
                foreach (var order in orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.Customer;
                    worksheet.Cell(currentRow, 2).Value = order.ModelName;
                    worksheet.Cell(currentRow, 3).Value = order.Color;
                    worksheet.Cell(currentRow, 4).Value = order.IsJIT ? "JIT" : "ATILDI";
                    worksheet.Cell(currentRow, 5).Value = order.OrderDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 6).Value = order.OrderCode;
                    worksheet.Cell(currentRow, 7).Value = order.Quantity;
                    worksheet.Cell(currentRow, 8).Value = order.GoodsDescription;
                    worksheet.Cell(currentRow, 9).Value = order.FabricSupplier;
                    
                    // KUMAŞ SEVK-ANLAŞILAN: Null ise STOK yazdırıyoruz
                    worksheet.Cell(currentRow, 10).Value = order.FabricArrivalAgreedDate.HasValue 
                        ? order.FabricArrivalAgreedDate.Value.ToString("dd.MM.yyyy") 
                        : "STOK";

                    worksheet.Cell(currentRow, 11).Value = order.FabricArrivalActualDate.HasValue 
                        ? order.FabricArrivalActualDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 12).Value = order.CuttingStartDate.HasValue 
                        ? order.CuttingStartDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 13).Value = order.CuttingEndDate.HasValue 
                        ? order.CuttingEndDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 14).Value = order.SewingStartDate.HasValue 
                        ? order.SewingStartDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 15).Value = order.SewingEndDate.HasValue 
                        ? order.SewingEndDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 16).Value = order.PackagingStartDate.HasValue 
                        ? order.PackagingStartDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 17).Value = order.LastInspectionDate.HasValue 
                        ? order.LastInspectionDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 18).Value = order.PackagingEndDate.HasValue 
                        ? order.PackagingEndDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 19).Value = order.DepartureDate.HasValue 
                        ? order.DepartureDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 20).Value = order.WarehouseArrivalDate.HasValue 
                        ? order.WarehouseArrivalDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 21).Value = order.LastInspectionDate.HasValue 
                        ? order.LastInspectionDate.Value.ToString("dd.MM.yyyy") 
                        : "";

                    worksheet.Cell(currentRow, 22).Value = order.SewingWorkshop;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CPS(A).xlsx");
                }
            }
        }
    }
}
