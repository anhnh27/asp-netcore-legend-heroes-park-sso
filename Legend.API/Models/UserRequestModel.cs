using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legend.API.Models
{
    public class UserRequestModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int Role { get; set; }
        public string ParkSeq { get; set; }
        public string Password { get; set; }

        public List<int> SelectedOperations { get; set; }
    }
}
