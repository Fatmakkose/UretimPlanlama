using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Data;
using UretimPlanlama.Models;
using ClosedXML.Excel;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace UretimPlanlama.Controllers
{
    [Authorize(Policy = "SiparisAccess")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.HasPermission("View"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.Where(f => f.IsActive).OrderBy(f => f.Name).ToList();
            ViewBag.Colors = _context.ColorDefs.OrderBy(c => c.Name).ToList();
            ViewBag.Customers = _context.Customers.OrderBy(c => c.Name).ToList();
            ViewBag.Brands = _context.Brands.OrderBy(b => b.Name).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();
            return View(orders);
        }

        public IActionResult Create()
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.Where(f => f.IsActive).OrderBy(f => f.Name).ToList();
            ViewBag.Customers = _context.Customers.OrderBy(c => c.Name).ToList();
            ViewBag.Colors = _context.ColorDefs.OrderBy(c => c.Name).ToList();
            ViewBag.Brands = _context.Brands.OrderBy(b => b.Name).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order)
        {
            if (!User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (ModelState.IsValid)
            {
                order.Status = "Yeni Kayıt";
                order.FabricStatus = "Bekleniyor";
                
                if (!string.IsNullOrEmpty(order.OrderMaterialsJson))
                {
                    try
                    {
                        var materials = System.Text.Json.JsonSerializer.Deserialize<List<OrderMaterial>>(order.OrderMaterialsJson);
                        if (materials != null)
                        {
                            foreach (var mat in materials)
                            {
                                if (!string.IsNullOrEmpty(mat.OzelliklerJson))
                                {
                                    var varyantAdi = mat.OzelliklerJson.Trim();
                                    var varyant = _context.StokVaryantlar.FirstOrDefault(v => v.StokKartiId == mat.StokKartiId && v.VaryantAdi == varyantAdi);
                                    if (varyant == null)
                                    {
                                        varyant = new StokVaryant { StokKartiId = mat.StokKartiId, VaryantAdi = varyantAdi, MevcutMiktar = 0 };
                                        _context.StokVaryantlar.Add(varyant);
                                        _context.SaveChanges();
                                    }
                                    mat.StokVaryantId = varyant.Id;
                                }
                                order.OrderMaterials.Add(mat);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("OrderMaterials JSON Deserialization failed: " + ex.Message + " | JSON: " + order.OrderMaterialsJson, ex);
                    }
                }

                _context.Add(order);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Sipariş oluşturuldu";
                return RedirectToAction(nameof(Index)); // Doğrudan sipariş yönetimi sayfasına yönlendir
            }
            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.Where(f => f.IsActive).OrderBy(f => f.Name).ToList();
            ViewBag.Customers = _context.Customers.OrderBy(c => c.Name).ToList();
            ViewBag.Brands = _context.Brands.OrderBy(b => b.Name).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.OrderBy(s => s.StokAdi).ToList();
            return View(order);
        }

        [HttpPost]
        public IActionResult CreateMultiple([FromBody] List<Order> orders)
        {
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }
            if (orders == null || orders.Count == 0)
                return Json(new { success = false, message = "Hiç sipariş satırı gönderilmedi." });

            try
            {
                foreach (var order in orders)
                {
                    order.Status = "Yeni Kayıt";
                    order.FabricStatus = "Bekleniyor";
                    
                    if (!string.IsNullOrEmpty(order.OrderMaterialsJson))
                    {
                        try
                        {
                            var materials = System.Text.Json.JsonSerializer.Deserialize<List<OrderMaterial>>(order.OrderMaterialsJson);
                            if (materials != null)
                            {
                                foreach (var mat in materials)
                                {
                                    if (!string.IsNullOrEmpty(mat.OzelliklerJson))
                                    {
                                        var varyantAdi = mat.OzelliklerJson.Trim();
                                        var varyant = _context.StokVaryantlar.FirstOrDefault(v => v.StokKartiId == mat.StokKartiId && v.VaryantAdi == varyantAdi);
                                        if (varyant == null)
                                        {
                                            varyant = new StokVaryant { StokKartiId = mat.StokKartiId, VaryantAdi = varyantAdi, MevcutMiktar = 0 };
                                            _context.StokVaryantlar.Add(varyant);
                                            _context.SaveChanges();
                                        }
                                        mat.StokVaryantId = varyant.Id;
                                    }
                                    mat.OrderId = order.Id; // Will be set by EF when order is saved, but we add to collection
                                    order.OrderMaterials.Add(mat);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("OrderMaterials JSON Deserialization failed: " + ex.Message + " | JSON: " + order.OrderMaterialsJson, ex);
                        }
                    }
                    
                    _context.Add(order);
                }
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Sipariş oluşturuldu";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null) msg += " | " + ex.InnerException.Message;
                return Json(new { success = false, message = msg });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }
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
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }
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
        public IActionResult GetDetail(int id)
        {
            if (!User.HasPermission("View"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }
            var order = _context.Orders
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokKarti)
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokVaryant)
                .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }
            if (order.OrderMaterials != null && order.OrderMaterials.Any())
            {
                order.OrderMaterialsJson = System.Text.Json.JsonSerializer.Serialize(order.OrderMaterials.Select(m => new {
                    m.StokKartiId,
                    StokKodu = m.StokKarti?.StokKodu,
                    StokAdi = m.StokKarti?.StokAdi,
                    Kategori = m.StokKarti?.Kategori,
                    m.StokVaryantId,
                    VaryantAdi = m.StokVaryant?.VaryantAdi,
                    m.Miktar,
                    m.BirimFiyat,
                    m.Aciklama,
                    m.OzelliklerJson
                }));
            }
            order.OrderMaterials = null!;
            return Json(new { success = true, data = order });
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
                worksheet.Cell(currentRow, 18).Value = "Asorti Sayısı";
                worksheet.Cell(currentRow, 19).Value = "Nihai Toplam Miktar";
                worksheet.Cell(currentRow, 20).Value = "Bölge";
                worksheet.Cell(currentRow, 21).Value = "JIT";
                worksheet.Cell(currentRow, 22).Value = "Dinamik Açık Adet Dağılımı";
                worksheet.Cell(currentRow, 23).Value = "Dinamik Asorti Dağılımı";

                var headerRange = worksheet.Range(1, 1, 1, 23);
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
                    worksheet.Cell(currentRow, 22).Value = FormatJsonDistribution(order.SizeDistributionJson, order, false);
                    worksheet.Cell(currentRow, 23).Value = FormatJsonDistribution(order.AsortiDistributionJson, order, true);
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!User.HasPermission("View") && !User.HasPermission("Write"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            var order = _context.Orders
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokKarti)
                .Include(o => o.OrderMaterials)
                    .ThenInclude(m => m.StokVaryant)
                .FirstOrDefault(o => o.Id == id);
                
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Workshops = _context.Workshops.Where(w => w.IsActive).OrderBy(w => w.Name).ToList();
            ViewBag.Fabricators = _context.Fabricators.Where(f => f.IsActive).OrderBy(f => f.Name).ToList();
            ViewBag.Customers = _context.Customers.OrderBy(c => c.Name).ToList();
            ViewBag.Colors = _context.ColorDefs.OrderBy(c => c.Name).ToList();
            ViewBag.Brands = _context.Brands.OrderBy(b => b.Name).ToList();
            ViewBag.StokKartlari = _context.StokKartlari.Where(s => s.Aktif).OrderBy(s => s.StokAdi).ToList();

            // Setup JSON string for existing materials so frontend can parse it
            if (order.OrderMaterials != null && order.OrderMaterials.Any())
            {
                var materialsData = order.OrderMaterials.Select(m => new
                {
                    m.StokKartiId,
                    m.Miktar,
                    m.Aciklama,
                    m.OzelliklerJson
                }).ToList();
                order.OrderMaterialsJson = System.Text.Json.JsonSerializer.Serialize(materialsData);
            }
            else
            {
                order.OrderMaterialsJson = "[]";
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult Edit([FromBody] Order updatedOrder)
        {
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }

            if (updatedOrder == null)
            {
                return Json(new { success = false, message = "Geçersiz sipariş verisi." });
            }

            try
            {
                var existingOrder = _context.Orders.Include(o => o.OrderMaterials).FirstOrDefault(o => o.Id == updatedOrder.Id);
                if (existingOrder == null)
                {
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                }

                // Genel özellikleri güncelle
                existingOrder.OrderDate = updatedOrder.OrderDate;
                existingOrder.OrderCode = updatedOrder.OrderCode;
                existingOrder.PaymentMethod = updatedOrder.PaymentMethod;
                existingOrder.ManufacturerCompany = updatedOrder.ManufacturerCompany;
                existingOrder.Customer = updatedOrder.Customer;
                existingOrder.ModelName = updatedOrder.ModelName;
                existingOrder.GoodsDescription = updatedOrder.GoodsDescription;
                existingOrder.InspectionType = updatedOrder.InspectionType;
                existingOrder.InspectionDate = updatedOrder.InspectionDate;
                existingOrder.FabricSupplier = updatedOrder.FabricSupplier;
                existingOrder.DeliveryPlace = updatedOrder.DeliveryPlace;
                existingOrder.Color = updatedOrder.Color;
                
                // Add new color to ColorDefs if it doesn't exist
                if (!string.IsNullOrWhiteSpace(updatedOrder.Color))
                {
                    bool colorExists = _context.ColorDefs.Any(c => c.Name == updatedOrder.Color);
                    if (!colorExists)
                    {
                        _context.ColorDefs.Add(new UretimPlanlama.Models.ColorDef { Name = updatedOrder.Color });
                    }
                }

                existingOrder.Brand = updatedOrder.Brand;
                existingOrder.IsJIT = updatedOrder.IsJIT;
                existingOrder.SalesRegion = updatedOrder.SalesRegion;
                existingOrder.PlannedPackagingEndDate = updatedOrder.PlannedPackagingEndDate;

                // Beden ve miktar bilgilerini güncelle
                existingOrder.SizeS = updatedOrder.SizeS;
                existingOrder.SizeM = updatedOrder.SizeM;
                existingOrder.SizeL = updatedOrder.SizeL;
                existingOrder.SizeXL = updatedOrder.SizeXL;
                existingOrder.Size2XL = updatedOrder.Size2XL;
                existingOrder.Size3XL = updatedOrder.Size3XL;

                existingOrder.AsortiSizeS = updatedOrder.AsortiSizeS;
                existingOrder.AsortiSizeM = updatedOrder.AsortiSizeM;
                existingOrder.AsortiSizeL = updatedOrder.AsortiSizeL;
                existingOrder.AsortiSizeXL = updatedOrder.AsortiSizeXL;
                existingOrder.AsortiSize2XL = updatedOrder.AsortiSize2XL;
                existingOrder.AsortiSize3XL = updatedOrder.AsortiSize3XL;

                existingOrder.AsortiCount = updatedOrder.AsortiCount;
                existingOrder.Quantity = updatedOrder.Quantity;

                existingOrder.SizeDistributionJson = updatedOrder.SizeDistributionJson;
                existingOrder.AsortiDistributionJson = updatedOrder.AsortiDistributionJson;
                
                // Keep existing ProductionJson but update KNN Revize Termin if provided
                if (!string.IsNullOrEmpty(updatedOrder.ProductionJson))
                {
                    existingOrder.ProductionJson = updatedOrder.ProductionJson;
                }

                // Aksesuar ve Tela Bilgileri
                existingOrder.SelectedAccessoriesJson = updatedOrder.SelectedAccessoriesJson;
                existingOrder.FabricsJson = updatedOrder.FabricsJson;
                existingOrder.UnitFabricMeterage = updatedOrder.UnitFabricMeterage;
                existingOrder.FabricUnit = updatedOrder.FabricUnit;
                existingOrder.WastageRate = updatedOrder.WastageRate;
                existingOrder.LargeButtonCount = updatedOrder.LargeButtonCount;
                existingOrder.SmallButtonCount = updatedOrder.SmallButtonCount;
                existingOrder.KusakAstarGram = updatedOrder.KusakAstarGram;
                existingOrder.KusakTelaRenk = updatedOrder.KusakTelaRenk;
                existingOrder.KusakTelaTipi = updatedOrder.KusakTelaTipi;
                existingOrder.YakaAstarGram = updatedOrder.YakaAstarGram;
                existingOrder.YakaTelaRenk = updatedOrder.YakaTelaRenk;
                existingOrder.YakaTelaTipi = updatedOrder.YakaTelaTipi;
                existingOrder.MansetAstarGram = updatedOrder.MansetAstarGram;
                existingOrder.MansetTelaRenk = updatedOrder.MansetTelaRenk;
                existingOrder.MansetTelaTipi = updatedOrder.MansetTelaTipi;
                existingOrder.KapakAstarGram = updatedOrder.KapakAstarGram;
                existingOrder.KapakTelaRenk = updatedOrder.KapakTelaRenk;
                existingOrder.KapakTelaTipi = updatedOrder.KapakTelaTipi;
                existingOrder.BossAstarGram = updatedOrder.BossAstarGram;
                existingOrder.BossTelaRenk = updatedOrder.BossTelaRenk;
                existingOrder.BossTelaTipi = updatedOrder.BossTelaTipi;
                existingOrder.PatAstarGram = updatedOrder.PatAstarGram;
                existingOrder.PatTelaRenk = updatedOrder.PatTelaRenk;
                existingOrder.PatTelaTipi = updatedOrder.PatTelaTipi;
                existingOrder.HasPriceCard = updatedOrder.HasPriceCard;
                existingOrder.HasWashingInstruction = updatedOrder.HasWashingInstruction;
                existingOrder.HasInnerBarcode = updatedOrder.HasInnerBarcode;
                existingOrder.HasYokeLabel = updatedOrder.HasYokeLabel;
                existingOrder.HasFifLabel = updatedOrder.HasFifLabel;
                existingOrder.HasOtherCard = updatedOrder.HasOtherCard;

                // Finansal özellikleri güncelle
                existingOrder.ComponentUnitPrice = updatedOrder.ComponentUnitPrice;
                existingOrder.UnitPrice = updatedOrder.UnitPrice;
                existingOrder.TotalAmount = updatedOrder.TotalAmount;
                existingOrder.VatAmount = updatedOrder.VatAmount;
                existingOrder.TotalAmountWithVat = updatedOrder.TotalAmountWithVat;

                // Sipariş malzemelerini güncelle (OrderMaterialsJson'dan)
                if (updatedOrder.OrderMaterialsJson != null)
                {
                    try
                    {
                        var newMaterials = System.Text.Json.JsonSerializer.Deserialize<List<OrderMaterial>>(updatedOrder.OrderMaterialsJson);
                        
                        _context.OrderMaterials.RemoveRange(existingOrder.OrderMaterials);
                        existingOrder.OrderMaterials.Clear();
                        
                        if (newMaterials != null)
                        {
                            foreach (var mat in newMaterials)
                            {
                                if (!string.IsNullOrEmpty(mat.OzelliklerJson))
                                {
                                    var varyantAdi = mat.OzelliklerJson.Trim();
                                    var varyant = _context.StokVaryantlar.FirstOrDefault(v => v.StokKartiId == mat.StokKartiId && v.VaryantAdi == varyantAdi);
                                    if (varyant == null)
                                    {
                                        varyant = new StokVaryant { StokKartiId = mat.StokKartiId, VaryantAdi = varyantAdi, MevcutMiktar = 0 };
                                        _context.StokVaryantlar.Add(varyant);
                                        _context.SaveChanges();
                                    }
                                    mat.StokVaryantId = varyant.Id;
                                }
                                mat.OrderId = existingOrder.Id;
                                existingOrder.OrderMaterials.Add(mat);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("OrderMaterials JSON Deserialization failed in Edit: " + ex.Message + " | JSON: " + updatedOrder.OrderMaterialsJson, ex);
                    }
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Sipariş güncellendi";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }
            var order = _context.Orders.Include(o => o.OrderMaterials).FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                if (order.OrderMaterials != null && order.OrderMaterials.Any())
                {
                    _context.OrderMaterials.RemoveRange(order.OrderMaterials);
                }
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }

        [HttpPost]
        public IActionResult DeleteMultiple([FromBody] List<int> ids)
        {
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }
            if (ids == null || !ids.Any()) return Json(new { success = false, message = "Geçersiz işlem." });

            var orders = _context.Orders.Include(o => o.OrderMaterials).Where(o => ids.Contains(o.Id)).ToList();
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order.OrderMaterials != null && order.OrderMaterials.Any())
                    {
                        _context.OrderMaterials.RemoveRange(order.OrderMaterials);
                    }
                }
                _context.Orders.RemoveRange(orders);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }

        [HttpPost]
        public IActionResult CloneOrder(int id)
        {
            if (!User.HasPermission("Write"))
            {
                return Json(new { success = false, message = "Yetkiniz yetersiz." });
            }

            var order = _context.Orders.Include(o => o.OrderMaterials).AsNoTracking().FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.Id = 0;
                order.Color = (order.Color ?? "Yeni Renk") + " (Kopya)";
                foreach (var mat in order.OrderMaterials)
                {
                    mat.Id = 0;
                    mat.OrderId = 0;
                }

                _context.Orders.Add(order);
                _context.SaveChanges();

                return Json(new { success = true, newId = order.Id });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }

        private string FormatJsonDistribution(string? json, Order order, bool isAsorti)
        {
            if (string.IsNullOrEmpty(json))
            {
                var list = new List<string>();
                if (isAsorti)
                {
                    if (order.AsortiSizeS > 0) list.Add($"S: {order.AsortiSizeS}");
                    if (order.AsortiSizeM > 0) list.Add($"M: {order.AsortiSizeM}");
                    if (order.AsortiSizeL > 0) list.Add($"L: {order.AsortiSizeL}");
                    if (order.AsortiSizeXL > 0) list.Add($"XL: {order.AsortiSizeXL}");
                    if (order.AsortiSize2XL > 0) list.Add($"2XL: {order.AsortiSize2XL}");
                    if (order.AsortiSize3XL > 0) list.Add($"3XL: {order.AsortiSize3XL}");
                }
                else
                {
                    if (order.SizeS > 0) list.Add($"S: {order.SizeS}");
                    if (order.SizeM > 0) list.Add($"M: {order.SizeM}");
                    if (order.SizeL > 0) list.Add($"L: {order.SizeL}");
                    if (order.SizeXL > 0) list.Add($"XL: {order.SizeXL}");
                    if (order.Size2XL > 0) list.Add($"2XL: {order.Size2XL}");
                    if (order.Size3XL > 0) list.Add($"3XL: {order.Size3XL}");
                }
                return string.Join(", ", list);
            }

            try
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                if (dict == null || dict.Count == 0) return string.Empty;
                return string.Join(", ", dict.Select(kv => $"{kv.Key}: {kv.Value}"));
            }
            catch
            {
                return string.Empty;
            }
        }

        [HttpGet]
        public IActionResult GetOrderMaterials(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı." });

            var materials = new List<object>();

            if (order.UnitFabricMeterage > 0)
            {
                var total = order.Quantity * order.UnitFabricMeterage;
                materials.Add(new { ad = "Kumaş", miktar = total, birim = order.FabricUnit ?? "Metre", kategori = "Kumaş" });
            }

            if (order.LargeButtonCount > 0)
                materials.Add(new { ad = "Büyük Düğme (24/Boy)", miktar = order.Quantity * order.LargeButtonCount, birim = "Adet", kategori = "Düğme" });
            
            if (order.SmallButtonCount > 0)
                materials.Add(new { ad = "Küçük Düğme (14/Boy)", miktar = order.Quantity * order.SmallButtonCount, birim = "Adet", kategori = "Düğme" });

            if (order.HasPriceCard) materials.Add(new { ad = "Fiyat Kartı", miktar = order.Quantity, birim = "Adet", kategori = "Etiket" });
            if (order.HasWashingInstruction) materials.Add(new { ad = "Yıkama Talimatı", miktar = order.Quantity, birim = "Adet", kategori = "Etiket" });
            if (order.HasInnerBarcode) materials.Add(new { ad = "İç Barkod", miktar = order.Quantity, birim = "Adet", kategori = "Etiket" });
            if (order.HasYokeLabel) materials.Add(new { ad = "Roba Etiketi", miktar = order.Quantity, birim = "Adet", kategori = "Etiket" });
            if (order.HasFifLabel) materials.Add(new { ad = "Fif Etiketi", miktar = order.Quantity, birim = "Adet", kategori = "Etiket" });
            if (order.HasOtherCard) materials.Add(new { ad = "Diğer Kart", miktar = order.Quantity, birim = "Adet", kategori = "Etiket" });

            if (!string.IsNullOrEmpty(order.KusakTelaTipi)) materials.Add(new { ad = "Kuşak Tela (" + order.KusakTelaTipi + ")", miktar = order.Quantity, birim = "Adet", kategori = "Tela" });
            if (!string.IsNullOrEmpty(order.YakaTelaTipi)) materials.Add(new { ad = "Yaka Tela (" + order.YakaTelaTipi + ")", miktar = order.Quantity, birim = "Adet", kategori = "Tela" });
            if (!string.IsNullOrEmpty(order.MansetTelaTipi)) materials.Add(new { ad = "Manşet Tela (" + order.MansetTelaTipi + ")", miktar = order.Quantity, birim = "Adet", kategori = "Tela" });
            if (!string.IsNullOrEmpty(order.KapakTelaTipi)) materials.Add(new { ad = "Kapak Tela (" + order.KapakTelaTipi + ")", miktar = order.Quantity, birim = "Adet", kategori = "Tela" });
            if (!string.IsNullOrEmpty(order.BossTelaTipi)) materials.Add(new { ad = "Boss Tela (" + order.BossTelaTipi + ")", miktar = order.Quantity, birim = "Adet", kategori = "Tela" });
            if (!string.IsNullOrEmpty(order.PatTelaTipi)) materials.Add(new { ad = "Pat Tela (" + order.PatTelaTipi + ")", miktar = order.Quantity, birim = "Adet", kategori = "Tela" });

            return Json(new { success = true, materials = materials });
        }
    }
}
