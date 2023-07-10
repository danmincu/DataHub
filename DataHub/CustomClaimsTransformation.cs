using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace DataHub
{
    public class CustomClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            var client_roleClaim = principal.Claims.FirstOrDefault(c => c.Type == "client_role");
            if (client_roleClaim != null)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, client_roleClaim.Value));
            }

            return Task.FromResult(principal);
        }
    }
}