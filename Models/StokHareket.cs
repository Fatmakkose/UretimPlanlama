using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class StokHareket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StokKartiId { get; set; }

        [Required(ErrorMessage = "İşlem tarihi zorunludur.")]
        public DateTime IslemTarihi { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Hareket tipi zorunludur.")]
        [MaxLength(30)]
        public string HareketTipi { get; set; } = "Giriş"; // "Giriş" | "Çıkış" | "Fire" | "Sayım Düzeltme"

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Miktar { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KalanMiktar { get; set; }

        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [MaxLength(200)]
        public string? Tedarikci { get; set; }

        public int? OrderId { get; set; }

        [MaxLength(100)]
        public string? BelgeNo { get; set; } // İrsaliye / Fatura no

        public bool IsApproved { get; set; } = false;

        // Navigation
        [ForeignKey("StokKartiId")]
        public StokKarti? StokKarti { get; set; }

        public int? StokVaryantId { get; set; }

        [ForeignKey("StokVaryantId")]
        public StokVaryant? StokVaryant { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
