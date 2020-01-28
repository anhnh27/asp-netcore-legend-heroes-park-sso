using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Legend.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
        // Add additional profile data for application users by adding properties to this class
        public ICollection<IdentityUserClaim<string>> Claims { get; set; } = new List<IdentityUserClaim<string>>();
    }
}
