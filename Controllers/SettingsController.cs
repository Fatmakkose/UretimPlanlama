using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string FullName, string RoleTitle, string Email, string Phone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.FullName = FullName;
                user.RoleTitle = RoleTitle;
                user.Email = Email;
                user.PhoneNumber = Phone;
                
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                }
            }
            return RedirectToAction("Profile");
        }
    }
}
