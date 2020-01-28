using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Legend.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Legend.Identity.Custom
{
    public class CustomProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> _userManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var identity = new ClaimsIdentity();

            var user = await _userManager.GetUserAsync(context.Subject);
            var claims = await _userManager.GetClaimsAsync(user);

            foreach (var claim in claims)
            {
                identity.AddClaims(new[]
                {
                    new Claim(claim.Type, claim.Value)
                });
            }

            context.IssuedClaims = identity.Claims.ToList(); 
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //>Processing
            var user = await _userManager.GetUserAsync(context.Subject);

            context.IsActive = (user != null) && user.IsActive;
        }
    }
}
