using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Accessory
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Aksesuar adı zorunludur.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Aksesuar tipi zorunludur.")]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Birim zorunludur.")]
        [MaxLength(20)]
        public string Unit { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? StockCode { get; set; }
    }
}
