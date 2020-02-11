using System.Collections.Generic;

namespace Legend.Admin.Models
{
    public class UserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string ParkSeq { get; set; }
        public string Password { get; set; }

        public IEnumerable<int> SelectedOperations { get; set; }
    }
}