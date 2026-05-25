using System;
using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
        
        public string? Type { get; set; } // "Kesim", "Dikim", "Kumaş", "Paket", "Sevkiyat" vb.
        
        public string? OrderCode { get; set; }
    }
}
