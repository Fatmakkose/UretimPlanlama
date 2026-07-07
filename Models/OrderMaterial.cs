using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class OrderMaterial
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        public int StokKartiId { get; set; }
        
        [ForeignKey("StokKartiId")]
        public StokKarti StokKarti { get; set; } = null!;

        public int? StokVaryantId { get; set; }

        [ForeignKey("StokVaryantId")]
        public StokVaryant? StokVaryant { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Miktar { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BirimFiyat { get; set; }

        public string? Aciklama { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualQuantity { get; set; } = 0;

        public bool IsApproved { get; set; } = false;

        public string? OzelliklerJson { get; set; } // Siparişe özel dinamik alan değerleri
    }
}
