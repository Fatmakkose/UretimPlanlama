using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Workshop
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Atölye adı zorunludur.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Atölye tipi zorunludur.")]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yetkili kişi zorunludur.")]
        [MaxLength(100)]
        public string AuthorizedPerson { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public int DailyCapacity { get; set; }
        public int MonthlyCapacity { get; set; }
        public int AnnualCapacity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
