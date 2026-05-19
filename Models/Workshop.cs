using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Workshop
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Atölye adı zorunludur.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Atölye tipi zorunludur.")]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required(ErrorMessage = "Yetkili kişi zorunludur.")]
        [MaxLength(100)]
        public string AuthorizedPerson { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
