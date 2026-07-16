using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UretimPlanlama.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string RoleTitle { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcı e-posta bildirimi almak istiyor mu?
        /// </summary>
        public bool ReceiveEmailNotifications { get; set; } = true;
    }
}
