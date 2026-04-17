using System.Security.Claims;

namespace GitFile.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(id))
                throw new Exception("User ID claim not found in JWT.");

            return Guid.Parse(id);
        }
    }
}
