using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using ClosedXML.Excel;
using System.IO;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            ViewBag.Colors = _context.ColorDefs.OrderBy(c => c.Name).ToList();
            return View(orders);
        }

        public IActionResult Create()
        {
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                order.Status = "Yeni Kayıt";
                order.FabricStatus = "Bekleniyor";
                _context.Add(order);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Yeni sipariş başarıyla eklendi.";
                return RedirectToAction(nameof(Index)); // Redirect directly to the order management page
            }
            ViewBag.Workshops = _context.Workshops.OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.OrderBy(f => f.Name).ToList();
            return View(order);
        }

        [HttpPost]
        public IActionResult CreateMultiple([FromBody] List<Order> orders)
        {
            if (orders == null || orders.Count == 0)
                return Json(new { success = false, message = "Hiç sipariş satırı gönderilmedi." });

            try
            {
                foreach (var order in orders)
                {
                    order.Status = "Yeni Kayıt";
                    order.FabricStatus = "Bekleniyor";
                    _context.Add(order);
                }
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"{orders.Count} adet yeni renk/sipariş başarıyla oluşturuldu.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
                return Json(new { success = true, message = "Sipariş durumu güncellendi." });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }

        [HttpPost]
        public IActionResult UpdateFabricStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.FabricStatus = status;
                _context.SaveChanges();
                return Json(new { success = true, message = "Kumaş durumu güncellendi." });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Siparişler");
                var currentRow = 1;

                // Başlıklar
                worksheet.Cell(currentRow, 1).Value = "Sipariş Tarihi";
                worksheet.Cell(currentRow, 2).Value = "Sipariş Kodu";
                worksheet.Cell(currentRow, 3).Value = "Model Numarası";
                worksheet.Cell(currentRow, 4).Value = "Model Adı";
                worksheet.Cell(currentRow, 5).Value = "Renk/Option";
                worksheet.Cell(currentRow, 6).Value = "S Beden (Açık)";
                worksheet.Cell(currentRow, 7).Value = "M Beden (Açık)";
                worksheet.Cell(currentRow, 8).Value = "L Beden (Açık)";
                worksheet.Cell(currentRow, 9).Value = "XL Beden (Açık)";
                worksheet.Cell(currentRow, 10).Value = "2XL Beden (Açık)";
                worksheet.Cell(currentRow, 11).Value = "3XL Beden (Açık)";
                worksheet.Cell(currentRow, 12).Value = "S Beden (Asorti)";
                worksheet.Cell(currentRow, 13).Value = "M Beden (Asorti)";
                worksheet.Cell(currentRow, 14).Value = "L Beden (Asorti)";
                worksheet.Cell(currentRow, 15).Value = "XL Beden (Asorti)";
                worksheet.Cell(currentRow, 16).Value = "2XL Beden (Asorti)";
                worksheet.Cell(currentRow, 17).Value = "3XL Beden (Asorti)";
                worksheet.Cell(currentRow, 18).Value = "Asorti Çarpanı";
                worksheet.Cell(currentRow, 19).Value = "Nihai Toplam Miktar";
                worksheet.Cell(currentRow, 20).Value = "Bölge";
                worksheet.Cell(currentRow, 21).Value = "JIT";

                var headerRange = worksheet.Range(1, 1, 1, 21);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Veriler
                foreach (var order in orders)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = order.OrderDate.ToString("dd.MM.yyyy");
                    worksheet.Cell(currentRow, 2).Value = order.OrderCode;
                    worksheet.Cell(currentRow, 3).Value = order.ModelNo;
                    worksheet.Cell(currentRow, 4).Value = order.ModelName;
                    worksheet.Cell(currentRow, 5).Value = order.Color;
                    
                    worksheet.Cell(currentRow, 6).Value = order.SizeS;
                    worksheet.Cell(currentRow, 7).Value = order.SizeM;
                    worksheet.Cell(currentRow, 8).Value = order.SizeL;
                    worksheet.Cell(currentRow, 9).Value = order.SizeXL;
                    worksheet.Cell(currentRow, 10).Value = order.Size2XL;
                    worksheet.Cell(currentRow, 11).Value = order.Size3XL;
                    
                    worksheet.Cell(currentRow, 12).Value = order.AsortiSizeS;
                    worksheet.Cell(currentRow, 13).Value = order.AsortiSizeM;
                    worksheet.Cell(currentRow, 14).Value = order.AsortiSizeL;
                    worksheet.Cell(currentRow, 15).Value = order.AsortiSizeXL;
                    worksheet.Cell(currentRow, 16).Value = order.AsortiSize2XL;
                    worksheet.Cell(currentRow, 17).Value = order.AsortiSize3XL;
                    
                    worksheet.Cell(currentRow, 18).Value = order.AsortiCount;
                    worksheet.Cell(currentRow, 19).Value = order.Quantity;
                    worksheet.Cell(currentRow, 20).Value = order.SalesRegion;
                    worksheet.Cell(currentRow, 21).Value = order.IsJIT ? "Evet" : "Hayır";
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Siparisler.xlsx");
                }
            }
        }
    }
}
