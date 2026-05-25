using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Phone { get; set; }
        
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [MaxLength(100)]
        public string? TaxOffice { get; set; }
        
        [MaxLength(50)]
        public string? TaxNumber { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
