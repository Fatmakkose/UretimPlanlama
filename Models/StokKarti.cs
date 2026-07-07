using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class StokKarti
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Stok kodu zorunludur.")]
        [MaxLength(50)]
        public string StokKodu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stok adı zorunludur.")]
        [MaxLength(200)]
        public string StokAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori zorunludur.")]
        [MaxLength(50)]
        public string Kategori { get; set; } = "Kumaş"; // "Kumaş" | "Aksesuar" | "İplik" | "Tela" | "Düğme" | "Etiket" | "Diğer"

        [Required(ErrorMessage = "Birim zorunludur.")]
        [MaxLength(20)]
        public string Birim { get; set; } = "Metre"; // "Metre" | "Kg" | "Adet" | "Top"

        [Column(TypeName = "decimal(18,2)")]
        public decimal MevcutMiktar { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumMiktar { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BirimFiyat { get; set; }

        [MaxLength(255)]
        public string? GorselUrl { get; set; } // Ürün Görseli

        [MaxLength(500)]
        public string? Aciklama { get; set; }

        public string? OzelliklerJson { get; set; } // Dinamik Özellikler JSON

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public bool Aktif { get; set; } = true;

        // Navigation
        public ICollection<StokVaryant> Varyantlar { get; set; } = new List<StokVaryant>();
        public ICollection<StokHareket> Hareketler { get; set; } = new List<StokHareket>();
    }
}
