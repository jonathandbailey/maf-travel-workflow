using System.Security.Claims;

namespace Travel.Application.Api.Extensions
{
    public static class UserPrincipalExtensions
    {
        public static Guid Id(this ClaimsPrincipal user)
        {
            // Return a static user until Azure B2C is added
            return Guid.Parse("B4C361C4-460C-4B1D-8DC7-34D5F3595AD1");
        }
    }
}