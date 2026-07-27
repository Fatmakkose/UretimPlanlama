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
    [Authorize(Policy = "PlanlamaAccess")]
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
            return View(orders);
        }

        [HttpGet]
        public IActionResult Plan(int id)
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            var order = _context.Orders.Include(o => o.OrderMaterials).ThenInclude(m => m.StokKarti).FirstOrDefault(o => o.Id == id);
            
            if (order == null) return NotFound();

            ViewBag.AllOrders = orders;
            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.Where(f => f.IsActive).OrderBy(f => f.Name).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.ToList();
            return View(order);
        }

        [HttpPost]
        public IActionResult UpdatePlan(Order orderData, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var order = _context.Orders.Find(orderData.Id);
            if (order != null)
            {
                if (form.ContainsKey("FabricSupplier")) order.FabricSupplier = orderData.FabricSupplier;
                if (form.ContainsKey("FabricArrivalAgreedDate")) order.FabricArrivalAgreedDate = orderData.FabricArrivalAgreedDate;
                
                if (form.ContainsKey("PlannedCuttingStartDate")) order.PlannedCuttingStartDate = orderData.PlannedCuttingStartDate;
                if (form.ContainsKey("PlannedCuttingEndDate")) order.PlannedCuttingEndDate = orderData.PlannedCuttingEndDate;

                if (form.ContainsKey("SewingWorkshop")) order.SewingWorkshop = orderData.SewingWorkshop;
                if (form.ContainsKey("PlannedSewingStartDate")) order.PlannedSewingStartDate = orderData.PlannedSewingStartDate;
                if (form.ContainsKey("PlannedSewingEndDate")) order.PlannedSewingEndDate = orderData.PlannedSewingEndDate;

                if (form.ContainsKey("PlannedPackagingStartDate")) order.PlannedPackagingStartDate = orderData.PlannedPackagingStartDate;
                if (form.ContainsKey("PlannedPackagingEndDate")) order.PlannedPackagingEndDate = orderData.PlannedPackagingEndDate;
                if (form.ContainsKey("PlannedLastInspectionDate")) order.PlannedLastInspectionDate = orderData.PlannedLastInspectionDate;

                if (form.ContainsKey("UnitCost")) order.UnitCost = orderData.UnitCost;
                if (form.ContainsKey("UnitPrice")) order.UnitPrice = orderData.UnitPrice;

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
                if (form.ContainsKey("ActualFabricMeterage")) order.ActualFabricMeterage = orderData.ActualFabricMeterage;
                if (form.ContainsKey("ActualFabricQty")) order.ActualFabricQty = orderData.ActualFabricQty;
                
                if (form.ContainsKey("FabricArrivalAgreedDate")) order.FabricArrivalAgreedDate = orderData.FabricArrivalAgreedDate;
                if (form.ContainsKey("FabricArrivalActualDate")) order.FabricArrivalActualDate = orderData.FabricArrivalActualDate;

                // Extra fields in PurchasingMaterialsJson
                var dict = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(orderData.PurchasingMaterialsJson))
                {
                    try {
                        dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(orderData.PurchasingMaterialsJson) ?? new Dictionary<string, string>();
                    } catch {}
                }

                string globalFabricSupplier = null;
                foreach (var k in form.Keys) {
                    if (k.StartsWith("pur_")) {
                        dict[k] = form[k].ToString();
                        if (k.EndsWith("_kumasci") && !string.IsNullOrEmpty(form[k].ToString())) {
                            globalFabricSupplier = form[k].ToString();
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(globalFabricSupplier)) {
                    order.FabricSupplier = globalFabricSupplier;
                } else if (form.ContainsKey("FabricSupplier")) {
                    order.FabricSupplier = orderData.FabricSupplier;
                }
                
                order.PurchasingMaterialsJson = System.Text.Json.JsonSerializer.Serialize(dict);
                
                // Satın Alma Tamamlandı mı?
                order.IsPurchasingCompleted = orderData.IsPurchasingCompleted;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Satın Alma planı güncellendi.";
                TempData["ActiveTab"] = "uretim";
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
                if (!string.IsNullOrEmpty(order.SampleTestJson))
                {
                    try {
                        dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.SampleTestJson) ?? new Dictionary<string, string>();
                    } catch {}
                }

                foreach (var k in form.Keys) {
                    if (k.StartsWith("sample_")) {
                        dict[k] = form[k].ToString();
                    }
                }

                if (form.ContainsKey("IsSampleTestCompleted")) {
                    order.IsSampleTestCompleted = form["IsSampleTestCompleted"] == "true" || form["IsSampleTestCompleted"].ToString().Contains("true");
                } else {
                    order.IsSampleTestCompleted = false;
                }

                order.SampleTestJson = System.Text.Json.JsonSerializer.Serialize(dict);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Numune ve Test planı güncellendi.";
                TempData["ActiveTab"] = "satinalma";
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

                // Atölye seçimi SewingWorkshop alanına da kaydediliyor (Atölye Tanım sayfası bu alandan okur)
                if (dict.TryGetValue("prod_dikim_atolyesi", out var dikimAtolye) && !string.IsNullOrEmpty(dikimAtolye))
                {
                    order.SewingWorkshop = dikimAtolye;
                }
                
                if (form.ContainsKey("IsProductionCompleted")) {
                    order.IsProductionCompleted = form["IsProductionCompleted"] == "true";
                } else {
                    order.IsProductionCompleted = false;
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Üretim planı başarıyla kaydedildi. Süreç Takip sayfasına yönlendirildiniz.";
                return RedirectToAction("Track", "ProcessTracking", new { id = order.Id });
            }
            return NotFound();
        }
        [HttpGet]
        public IActionResult Tracking(int? selectedId)
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            
            var completedOrders = orders.Where(o => o.Status == "Tamamlandı" || o.PackagingEndDate.HasValue).ToList();
            int totalCompleted = completedOrders.Count;
            int onTimeCompleted = completedOrders.Count(o => o.PlannedPackagingEndDate.HasValue && o.PackagingEndDate.HasValue && o.PackagingEndDate.Value.Date <= o.PlannedPackagingEndDate.Value.Date);
            double efficiency = totalCompleted > 0 ? Math.Round((double)onTimeCompleted / totalCompleted * 100, 1) : 0;
            
            ViewBag.EfficiencyPercentage = efficiency;
            ViewBag.TotalCompleted = totalCompleted;
            ViewBag.OnTimeCompleted = onTimeCompleted;

            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.Where(f => f.IsActive).OrderBy(f => f.Name).ToList();
            ViewBag.SelectedId = selectedId;
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateTracking(Order orderData, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var old = _context.Orders.AsNoTracking().FirstOrDefault(o => o.Id == orderData.Id);
            var order = _context.Orders.Find(orderData.Id);
            if (order != null)
            {
                var dictProd = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.ProductionJson))
                {
                    try { dictProd = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.ProductionJson) ?? new Dictionary<string, string>(); } catch {}
                }
                if (form.ContainsKey("actual_cutting")) dictProd["actual_cutting"] = form["actual_cutting"].ToString();
                if (form.ContainsKey("actual_sewing")) dictProd["actual_sewing"] = form["actual_sewing"].ToString();
                if (form.ContainsKey("actual_packaging")) dictProd["actual_packaging"] = form["actual_packaging"].ToString();
                order.ProductionJson = System.Text.Json.JsonSerializer.Serialize(dictProd);

                var dictPur = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.PurchasingMaterialsJson))
                {
                    try { dictPur = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.PurchasingMaterialsJson) ?? new Dictionary<string, string>(); } catch {}
                }
                if (form.ContainsKey("actual_pur_dugme_gelisi")) dictPur["actual_pur_dugme_gelisi"] = form["actual_pur_dugme_gelisi"].ToString();
                if (form.ContainsKey("actual_pur_dugme_miktar")) dictPur["actual_pur_dugme_miktar"] = form["actual_pur_dugme_miktar"].ToString();
                
                if (form.ContainsKey("actual_pur_dugme_dikis_ipi")) dictPur["actual_pur_dugme_dikis_ipi"] = form["actual_pur_dugme_dikis_ipi"].ToString();
                if (form.ContainsKey("actual_pur_dugme_dikis_ipi_miktar")) dictPur["actual_pur_dugme_dikis_ipi_miktar"] = form["actual_pur_dugme_dikis_ipi_miktar"].ToString();
                
                if (form.ContainsKey("actual_pur_etiket_gelisi")) dictPur["actual_pur_etiket_gelisi"] = form["actual_pur_etiket_gelisi"].ToString();
                if (form.ContainsKey("actual_pur_etiket_miktar")) dictPur["actual_pur_etiket_miktar"] = form["actual_pur_etiket_miktar"].ToString();
                
                if (form.ContainsKey("actual_pur_fiyat_kart_gelisi")) dictPur["actual_pur_fiyat_kart_gelisi"] = form["actual_pur_fiyat_kart_gelisi"].ToString();
                if (form.ContainsKey("actual_pur_fiyat_kart_miktar")) dictPur["actual_pur_fiyat_kart_miktar"] = form["actual_pur_fiyat_kart_miktar"].ToString();
                
                if (form.ContainsKey("actual_pur_yikama_ic_gelis")) dictPur["actual_pur_yikama_ic_gelis"] = form["actual_pur_yikama_ic_gelis"].ToString();
                if (form.ContainsKey("actual_pur_yikama_ic_miktar")) dictPur["actual_pur_yikama_ic_miktar"] = form["actual_pur_yikama_ic_miktar"].ToString();
                
                order.PurchasingMaterialsJson = System.Text.Json.JsonSerializer.Serialize(dictPur);

                var dictSample = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(order.SampleTestJson))
                {
                    try { dictSample = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(order.SampleTestJson) ?? new Dictionary<string, string>(); } catch {}
                }
                if (form.ContainsKey("actual_sample_kumas_ytesti")) dictSample["actual_sample_kumas_ytesti"] = form["actual_sample_kumas_ytesti"].ToString();
                if (form.ContainsKey("actual_sample_tuse_renk")) dictSample["actual_sample_tuse_renk"] = form["actual_sample_tuse_renk"].ToString();
                if (form.ContainsKey("actual_sample_dugme_renk")) dictSample["actual_sample_dugme_renk"] = form["actual_sample_dugme_renk"].ToString();
                if (form.ContainsKey("actual_sample_dugme_test")) dictSample["actual_sample_dugme_test"] = form["actual_sample_dugme_test"].ToString();
                if (form.ContainsKey("actual_sample_pp_onay")) dictSample["actual_sample_pp_onay"] = form["actual_sample_pp_onay"].ToString();
                order.SampleTestJson = System.Text.Json.JsonSerializer.Serialize(dictSample);
                if (form.ContainsKey("FabricArrivalActualDate")) order.FabricArrivalActualDate = orderData.FabricArrivalActualDate;
                if (form.ContainsKey("FabricMeterage")) order.FabricMeterage = orderData.FabricMeterage;
                if (form.ContainsKey("ActualFabricQty")) order.ActualFabricQty = orderData.ActualFabricQty;

                if (form.ContainsKey("CuttingStartDate")) order.CuttingStartDate = orderData.CuttingStartDate;
                if (form.ContainsKey("CuttingEndDate")) order.CuttingEndDate = orderData.CuttingEndDate;

                if (form.ContainsKey("SewingStartDate")) order.SewingStartDate = orderData.SewingStartDate;
                if (form.ContainsKey("SewingEndDate")) order.SewingEndDate = orderData.SewingEndDate;

                if (form.ContainsKey("PackagingStartDate")) order.PackagingStartDate = orderData.PackagingStartDate;
                if (form.ContainsKey("PackagingEndDate")) order.PackagingEndDate = orderData.PackagingEndDate;
                if (form.ContainsKey("LastInspectionDate")) order.LastInspectionDate = orderData.LastInspectionDate;

                if (form.ContainsKey("DepartureDate")) order.DepartureDate = orderData.DepartureDate;
                if (form.ContainsKey("WarehouseArrivalDate")) order.WarehouseArrivalDate = orderData.WarehouseArrivalDate;

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
    
        [HttpPost]
        public IActionResult UpdatePlannedCutting([FromBody] PlannedCuttingRequest request)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(request.Id);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.PlannedCuttingJson = request.PlannedCuttingJson;
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }

    public class PlannedCuttingRequest
    {
        public int Id { get; set; }
        public string PlannedCuttingJson { get; set; } = string.Empty;
    }
}
