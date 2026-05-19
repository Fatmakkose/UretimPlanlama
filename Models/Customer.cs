using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Phone { get; set; }
        
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [MaxLength(100)]
        public string? TaxInfo { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
