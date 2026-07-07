using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UretimPlanlama.Models
{
    public class StokVaryant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StokKartiId { get; set; }

        [ForeignKey("StokKartiId")]
        public StokKarti StokKarti { get; set; } = null!;

        [Required(ErrorMessage = "Varyant adı zorunludur.")]
        [MaxLength(200)]
        public string VaryantAdi { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MevcutMiktar { get; set; } = 0;

        public bool Aktif { get; set; } = true;
    }
}
