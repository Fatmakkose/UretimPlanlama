using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sipariş Tarihi zorunludur.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Sipariş Kodu zorunludur.")]
        public string OrderCode { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; }

        public string? ManufacturerCode { get; set; }

        [Required(ErrorMessage = "Model Adı zorunludur.")]
        public string ModelName { get; set; } = string.Empty;

        public string? ModelNo { get; set; } // Model Numarası


        public string? ManufacturerCompany { get; set; }

        [Required(ErrorMessage = "Alıcı alanı zorunludur.")]
        public string Customer { get; set; } = "LC Waikiki";

        public string? GoodsDescription { get; set; }

        public string? InspectionType { get; set; } // Yurt İçi / Yurt Dışı
        public DateTime? InspectionDate { get; set; }
        public DateTime? RevisedInspectionDate { get; set; } // Revize Termin Tarihi

        public string? Brand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FabricPrice { get; set; }

        public string? DeliveryPlace { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        public int Quantity { get; set; }

        [NotMapped]
        public int CalculatedQuantity
        {
            get
            {
                int qty = Quantity;
                if (!string.IsNullOrEmpty(PurchasingMaterialsJson))
                {
                    try
                    {
                        var purData = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(PurchasingMaterialsJson);
                        if (purData != null)
                        {
                            string c = string.IsNullOrEmpty(Color) ? "GENEL" : Color;
                            
                            // 1. İstenen Kumaş -> Çıkacak Adet
                            string target = purData.ContainsKey($"pur_color_{c}_beden_cikacak") ? purData[$"pur_color_{c}_beden_cikacak"] : null;
                            if (string.IsNullOrEmpty(target) && purData.ContainsKey("pur_color_GENEL_beden_cikacak"))
                                target = purData["pur_color_GENEL_beden_cikacak"];
                            
                            if (!string.IsNullOrEmpty(target) && int.TryParse(target.Replace(".", ""), out int val) && val > 0)
                                return val;

                            // 2. İhtiyaç Olan Kumaş -> Sipariş Adeti
                            target = purData.ContainsKey($"pur_color_{c}_siparis_adeti") ? purData[$"pur_color_{c}_siparis_adeti"] : null;
                            if (string.IsNullOrEmpty(target) && purData.ContainsKey("pur_color_GENEL_siparis_adeti"))
                                target = purData["pur_color_GENEL_siparis_adeti"];
                            
                            if (!string.IsNullOrEmpty(target) && int.TryParse(target.Replace(".", ""), out int val2) && val2 > 0)
                                return val2;
                        }
                    }
                    catch { }
                }
                return qty;
            }
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ComponentUnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? VatAmount { get; set; }

        public string? FabricSupplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmountWithVat { get; set; }

        // Ekstra
        public string? Color { get; set; }

        public string? OptionCode { get; set; } // Renk bazlı model/varyant numarası
        public string? OpenSpecialCode { get; set; } // Açık Adet Özel Kodu
        public string? AsortiSpecialCode { get; set; } // Asorti Adet Özel Kodu
        public string? SizeDistributionJson { get; set; } // Dinamik Açık Beden Dağılımı JSON
        public string? AsortiDistributionJson { get; set; } // Dinamik Asorti Dağılımı JSON
        public string? SelectedAccessoriesJson { get; set; } // Seçilen Aksesuarlar JSON
        public string? FabricStatus { get; set; }
        public string? ProductionPlace { get; set; }
        public string? Status { get; set; }

        public bool IsJIT { get; set; }
        public string? SalesRegion { get; set; }

        // --- PLANLAMA VE TAKİP ALANLARI (Aşama 2) ---

        // Kumaş ve Tedarik
        public DateTime? FabricArrivalAgreedDate { get; set; } // Kumaş Sevk-Anlaşılan
        public DateTime? FabricArrivalActualDate { get; set; } // Kumaş Geliş Tarihi
        public double? FabricMeterage { get; set; } // Gelen Metraj (m)

        // Kesim - Planlanan
        public DateTime? PlannedCuttingStartDate { get; set; } // Kesim Başlangıç Planlanan
        public DateTime? PlannedCuttingEndDate { get; set; } // Kesim Bitiş Planlanan

        // Kesim - Gerçekleşen
        public DateTime? CuttingStartDate { get; set; } // Kesim Başlangıç Gerçekleşen
        public DateTime? CuttingEndDate { get; set; } // Kesim Bitiş Gerçekleşen

        // Dikim - Planlanan
        public DateTime? PlannedSewingStartDate { get; set; } // Dikim Başlangıç Planlanan
        public DateTime? PlannedSewingEndDate { get; set; } // Dikim Bitiş Planlanan

        // Dikim - Gerçekleşen
        public string? SewingWorkshop { get; set; } // Dikim Atölyesi
        public DateTime? SewingStartDate { get; set; } // Dikim Başlangıç Gerçekleşen
        public DateTime? SewingEndDate { get; set; } // Dikim Bitiş Gerçekleşen

        // Paket ve Kalite - Planlanan
        public DateTime? PlannedPackagingStartDate { get; set; } // Paket Başlangıç Planlanan
        public DateTime? PlannedPackagingEndDate { get; set; } // Paket Bitiş Planlanan
        public DateTime? PlannedLastInspectionDate { get; set; } // Son Inspection Planlanan

        [NotMapped]
        public DateTime? EffectiveTerminDate {
            get {
                if (!string.IsNullOrEmpty(ProductionJson)) {
                    try {
                        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(ProductionJson);
                        if (dict != null && dict.ContainsKey("prod_knn_revize_termin") && !string.IsNullOrEmpty(dict["prod_knn_revize_termin"])) {
                            if (DateTime.TryParse(dict["prod_knn_revize_termin"], out DateTime revizeDate) || 
                                DateTime.TryParseExact(dict["prod_knn_revize_termin"], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out revizeDate)) 
                            {
                                return revizeDate;
                            }
                        }
                    } catch {}
                }
                return PlannedPackagingEndDate;
            }
        }

        // Paket ve Kalite - Gerçekleşen
        public DateTime? PackagingStartDate { get; set; } // Paket Başlangıç Gerçekleşen
        public DateTime? PackagingEndDate { get; set; } // Paket Bitiş Gerçekleşen
        public DateTime? LastInspectionDate { get; set; } // Son Inspection Gerçekleşen

        // Sevkiyat
        public DateTime? DepartureDate { get; set; } // Yola Çıkış
        public DateTime? WarehouseArrivalDate { get; set; } // Depo Varış

        // Finans ve Maliyet
        public decimal? UnitCost { get; set; } // Birim Maliyet
        public decimal? UnitPrice { get; set; } // Birim Satış Fiyatı

        // Beden Dağılımları (Açık Adet)
        public int SizeS { get; set; } = 0;
        public int SizeM { get; set; } = 0;
        public int SizeL { get; set; } = 0;
        public int SizeXL { get; set; } = 0;
        public int Size2XL { get; set; } = 0;
        public int Size3XL { get; set; } = 0;

        // Beden Dağılımları (Asorti Adet / Oran)
        public int AsortiSizeS { get; set; } = 0;
        public int AsortiSizeM { get; set; } = 0;
        public int AsortiSizeL { get; set; } = 0;
        public int AsortiSizeXL { get; set; } = 0;
        public int AsortiSize2XL { get; set; } = 0;
        public int AsortiSize3XL { get; set; } = 0;

        // Toplam Asorti Kutusu / Sayısı
        public int AsortiCount { get; set; } = 0; // Asorti Katsayısı/Lot Sayısı

        // --- SÜREÇ TAKİP ALANLARI (Aşama 3) ---
        public bool IsPurchasingApproved { get; set; } = false; // Satın Alma Onayı
        public string? MaterialDispatchJson { get; set; } // Malzeme Sevk Kayıtları
        public string? CuttingProcessJson { get; set; } // Günlük Kesim Kayıtları
        public string? PlannedCuttingJson { get; set; } // Kesim Planlaması Kayıtları
        public string? FileClosingJson { get; set; } // Dosya Kapama / Maliyet ve Kumaş Analizi Kayıtları
        public string? PackingListJson { get; set; } // Çeki Listesi Kayıtları JSON



        // --- MODEL DETAYLARI VE AKSESUAR / TELA BİLGİLERİ ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitFabricMeterage { get; set; } // Birim Kumaş Miktarı

        public string? FabricUnit { get; set; } // Kumaş Birimi (Metraj (m) veya Kg)

        public int? LargeButtonCount { get; set; } // Büyük Düğme Sayısı (24/ Boy Düğme)
        public int? SmallButtonCount { get; set; } // Küçük Düğme Sayısı (14/ Boy Düğme)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? WastageRate { get; set; } // Fire Oranı (%)

        public string? FabricsJson { get; set; } // Çoklu Kumaş Bilgileri JSON (Metraj, Düğme vb.)

        // Diğer Aksesuarlar (1'er Adet)
        public bool HasPriceCard { get; set; } // Fiyat Kartı
        public bool HasWashingInstruction { get; set; } // Yıkama Talimatı
        public bool HasInnerBarcode { get; set; } // İç Barkod
        public bool HasYokeLabel { get; set; } // Roba Etiketi
        public bool HasFifLabel { get; set; } // Fif Etiketi
        public bool HasOtherCard { get; set; } // Diğer Kart

        // --- ASTAR VE TELA BİLGİLERİ (CİNSİ / GRAM / RENK) ---
        public string? KusakAstarGram { get; set; }
        public string? KusakTelaRenk { get; set; }
        public string? KusakTelaTipi { get; set; }

        public string? YakaAstarGram { get; set; }
        public string? YakaTelaRenk { get; set; }
        public string? YakaTelaTipi { get; set; }

        public string? MansetAstarGram { get; set; }
        public string? MansetTelaRenk { get; set; }
        public string? MansetTelaTipi { get; set; }

        public string? KapakAstarGram { get; set; }
        public string? KapakTelaRenk { get; set; }
        public string? KapakTelaTipi { get; set; }

        public string? BossAstarGram { get; set; }
        public string? BossTelaRenk { get; set; }
        public string? BossTelaTipi { get; set; }

        public string? PatAstarGram { get; set; }
        public string? PatTelaRenk { get; set; }
        public string? PatTelaTipi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetFabricQty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualFabricQty { get; set; }

        // --- YENİ PLANLAMA (SATIN ALMA / ÜRETİM / NUMUNE) ALANLARI ---

        // Kumaş (Metraj)
        public double? PlannedFabricMeterage { get; set; } // Planlanan Kumaş Metraj (m)
        public double? ActualFabricMeterage { get; set; } // Gerçekleşen Kumaş Metraj (m)
        
        // Aksesuar Tedarik Durumları
        public string? ButtonStatus { get; set; } // Düğme durumu (Sipariş Edildi vb.)
        public string? MainLabelStatus { get; set; } // Ana Etiket durumu
        public string? WashingInstructionStatus { get; set; } // Yıkama Talimatı durumu
        public DateTime? AccessoryCompletionDate { get; set; } // Aksesuar Tamamlanma Tarihi

        // Malzemeler Tablosu (İplik, Tela, Askı vs.)
        public string? PurchasingMaterialsJson { get; set; } 

        // Numune/Test Bilgileri
        public string? SampleTestJson { get; set; } // Numune test bilgileri
        public string? TalosTestJson { get; set; } // LCW Kumaş Testleri (TALOS) JSON

        // Üretim Ekstra Bilgileri
        public string? ProductionJson { get; set; } // Üretim ekstra bilgileri (GS Gidişi, Termin vb.)

        // Aşama Durumları
        public bool IsPurchasingCompleted { get; set; } = false;
        public bool IsProductionCompleted { get; set; } = false;
        public bool IsSampleTestCompleted { get; set; } = false;

        [NotMapped]
        public string? OrderMaterialsJson { get; set; }

        public ICollection<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
    }
}
