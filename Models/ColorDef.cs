using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class ColorDef
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Renk adı zorunludur.")]
        [Display(Name = "Renk Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
    }
}
