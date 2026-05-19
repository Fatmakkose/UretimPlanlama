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
        public string OrderCode { get; set; }

        public string? PaymentMethod { get; set; }

        public string? ManufacturerCode { get; set; }

        [Required(ErrorMessage = "Model Adı zorunludur.")]
        public string ModelName { get; set; }

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
        public string? FabricStatus { get; set; } 
        public string? ProductionPlace { get; set; } 
        public string? Status { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetFabricQty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualFabricQty { get; set; } 
    }
}
