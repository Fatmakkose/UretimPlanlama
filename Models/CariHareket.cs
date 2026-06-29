using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class CariHareket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CariHesapId { get; set; }

        [Required(ErrorMessage = "İşlem tarihi zorunludur.")]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "İşlem tipi zorunludur.")]
        [MaxLength(20)]
        public string IslemTipi { get; set; } = "Borç"; // "Borç" | "Alacak"

        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KalanBakiye { get; set; }

        public int? OrderId { get; set; }

        public int? StokKartiId { get; set; } // Hangi stok alındı (Alış Faturası için)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Miktar { get; set; } // Fatura miktarı

        [MaxLength(100)]
        public string? BelgeNo { get; set; } // Fatura / Dekont numarası

        [MaxLength(255)]
        public string? EFaturaYolu { get; set; } // E-Fatura dosya yolu

        // Navigation
        [ForeignKey("CariHesapId")]
        public CariHesap? CariHesap { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("StokKartiId")]
        public StokKarti? StokKarti { get; set; }
    }
}
