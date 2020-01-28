using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Legend.API.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
        // Add additional profile data for application users by adding properties to this class
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
    }
}
