namespace Legend.Identity.Models
{
    public class RegisterViewModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Picture { get; set; }
        public string PhoneNumber { get; set; }
    }
}
