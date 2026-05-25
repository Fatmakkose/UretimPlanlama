using System.Security.Claims;

namespace UretimPlanlama
{
    public static class AuthorizationExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            if (user == null) return false;
            
            // Primary Admin role bypasses all permission checks automatically
            if (user.IsInRole("Admin")) return true;
            
            // Check direct user claims or role claims matching "Permission" type and the requested value
            return user.HasClaim(c => c.Type == "Permission" && c.Value == permission);
        }
    }
}
