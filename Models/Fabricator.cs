using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Fabricator
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kumaşçı adı zorunludur.")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
