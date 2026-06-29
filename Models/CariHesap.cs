using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class CariHesap
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hesap kodu zorunludur.")]
        [MaxLength(50)]
        public string HesapKodu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hesap adı zorunludur.")]
        [MaxLength(200)]
        public string HesapAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hesap tipi zorunludur.")]
        [MaxLength(50)]
        public string HesapTipi { get; set; } = "Müşteri"; // "Müşteri" | "Tedarikçi" | "Fason Atölye"

        [MaxLength(50)]
        public string? Telefon { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? VergiDairesi { get; set; }

        [MaxLength(50)]
        public string? VergiNumarasi { get; set; }

        [MaxLength(500)]
        public string? Adres { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bakiye { get; set; } = 0;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public bool Aktif { get; set; } = true;

        // Navigation
        public ICollection<CariHareket> Hareketler { get; set; } = new List<CariHareket>();
    }
}
