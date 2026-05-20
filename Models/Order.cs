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

        public string? Brand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FabricPrice { get; set; }

        public string? DeliveryPlace { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        public int Quantity { get; set; }

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
        public string? SizeDistributionJson { get; set; } // Dinamik Açık Beden Dağılımı JSON
        public string? AsortiDistributionJson { get; set; } // Dinamik Asorti Dağılımı JSON
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

        // Kesim
        public DateTime? CuttingStartDate { get; set; } // Kesim Başlangıç
        public DateTime? CuttingEndDate { get; set; } // Kesim Bitiş

        // Dikim
        public string? SewingWorkshop { get; set; } // Dikim Atölyesi
        public DateTime? SewingStartDate { get; set; } // Dikim Başlangıç
        public DateTime? SewingEndDate { get; set; } // Dikim Bitiş

        // Paket ve Kalite
        public DateTime? PackagingStartDate { get; set; } // Paket Başlangıç
        public DateTime? PackagingEndDate { get; set; } // Paket Bitiş
        public DateTime? LastInspectionDate { get; set; } // Son Inspection Tarihi

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
        public int AsortiCount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetFabricQty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualFabricQty { get; set; } 
    }
}
