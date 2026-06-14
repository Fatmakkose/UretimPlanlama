using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UretimPlanlama.Models;

namespace UretimPlanlama.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuthorizationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthorizationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Görünüm Modelleri (View Models)
        public class UserAuthorizationViewModel
        {
            public string UserId { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string RoleTitle { get; set; } = string.Empty;
            public string AssignedRole { get; set; } = string.Empty;
            public List<string> DirectPermissions { get; set; } = new();
            public List<string> InheritedPermissions { get; set; } = new();
        }

        public class RoleAuthorizationViewModel
        {
            public string RoleId { get; set; } = string.Empty;
            public string RoleName { get; set; } = string.Empty;
            public List<string> MappedPermissions { get; set; } = new();
        }

        public async Task<IActionResult> Index()
        {
            // 1. Tüm rolleri ve bunlarla eşlenmiş yetkileri (claim'leri) getir
            var roles = _roleManager.Roles.ToList();
            var roleViewModels = new List<RoleAuthorizationViewModel>();

            foreach (var r in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(r);
                var permissions = claims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();

                roleViewModels.Add(new RoleAuthorizationViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    MappedPermissions = permissions
                });
            }

           
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserAuthorizationViewModel>();

            foreach (var u in users)
            {
                var userRoles = await _userManager.GetRolesAsync(u);
                var primaryRole = userRoles.FirstOrDefault() ?? "Kullanıcı Yok";
                
                var directClaims = await _userManager.GetClaimsAsync(u);
                var directPermissions = directClaims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();

                var inheritedPermissions = new List<string>();
                if (!string.IsNullOrEmpty(primaryRole))
                {
                    var roleEntity = await _roleManager.FindByNameAsync(primaryRole);
                    if (roleEntity != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                        inheritedPermissions = roleClaims
                            .Where(c => c.Type == "Permission")
                            .Select(c => c.Value)
                            .ToList();
                    }
                }

                userViewModels.Add(new UserAuthorizationViewModel
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    RoleTitle = u.RoleTitle,
                    AssignedRole = primaryRole,
                    DirectPermissions = directPermissions,
                    InheritedPermissions = inheritedPermissions
                });
            }

            ViewBag.Roles = roleViewModels;
            ViewBag.Users = userViewModels;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password, string assignedRole, string roleTitle)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Ad Soyad, E-Posta ve Şifre alanlarının doldurulması zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            email = email.Trim();
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten mevcut.";
                return RedirectToAction(nameof(Index));
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName.Trim(),
                RoleTitle = string.IsNullOrWhiteSpace(roleTitle) ? assignedRole : roleTitle.Trim()
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(assignedRole) && await _roleManager.RoleExistsAsync(assignedRole))
                {
                    await _userManager.AddToRoleAsync(user, assignedRole);
                }

                if (assignedRole == "Admin" && user.RoleTitle == "Admin")
                {
                    user.RoleTitle = "Sistem Yöneticisi";
                    await _userManager.UpdateAsync(user);
                }
                else if (assignedRole == "User" && user.RoleTitle == "User")
                {
                    user.RoleTitle = "Kullanıcı";
                    await _userManager.UpdateAsync(user);
                }

                TempData["SuccessMessage"] = $"'{fullName}' kullanıcısı başarıyla oluşturuldu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı oluşturulurken bir hata oluştu: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserPermissions(string userId, string assignedRole, List<string> permissions)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Kullanıcının kimlik (identity) rolünü güncelle
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(assignedRole))
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (removeResult.Succeeded && !string.IsNullOrEmpty(assignedRole))
                {
                    await _userManager.AddToRoleAsync(user, assignedRole);
                }
            }

            // 2. Kullanıcının rol başlığı görüntüleme alanını güncelle (örn. Yönetici, Planlamacı, Operatör vb.)
            user.RoleTitle = assignedRole switch
            {
                "Admin" => "Sistem Yöneticisi",
                "User" => "Kullanıcı",
                _ => assignedRole
            };
            await _userManager.UpdateAsync(user);

            // 3. Kullanıcıya özel tanımlanmış yetki taleplerini (permission claims) güncelle
            var currentClaims = await _userManager.GetClaimsAsync(user);
            var permissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();
            
            foreach (var c in permissionClaims)
            {
                await _userManager.RemoveClaimAsync(user, c);
            }

            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    await _userManager.AddClaimAsync(user, new Claim("Permission", p));
                }
            }

            TempData["SuccessMessage"] = $"{user.FullName} yetkilendirmesi başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol ismi boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            roleName = roleName.Trim();

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["ErrorMessage"] = "Bu rol zaten mevcut.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"'{roleName}' rolü başarıyla oluşturuldu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Rol oluşturulurken hata meydana geldi: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, string roleName, List<string> permissions)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Rol bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Olası sorunları önlemek için sistem 'Admin' rol adının değiştirilmesini engelle
            if (role.Name != "Admin" && role.Name != "User" && !string.IsNullOrWhiteSpace(roleName) && role.Name != roleName)
            {
                role.Name = roleName.Trim();
                await _roleManager.UpdateAsync(role);
            }

            // Rol yetkilerini (claim'lerini) güncelle
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();
            
            foreach (var c in permissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, c);
            }

            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("Permission", p));
                }
            }

            TempData["SuccessMessage"] = $"'{role.Name}' rolü varsayılan yetkileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Rol bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Çekirdek sistem rollerini koru
            if (role.Name == "Admin" || role.Name == "User")
            {
                TempData["ErrorMessage"] = "Sistem rollerini ('Admin', 'User') silemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"'{role.Name}' rolü başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Rol silinemedi: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
