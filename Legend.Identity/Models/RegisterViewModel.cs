using System;
namespace Legend.Identity.Models
{
    public class RegisterViewModel
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Picture { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string ParkSeq { get; set; }
    }
}
