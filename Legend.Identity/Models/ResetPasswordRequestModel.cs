namespace Legend.Identity.Models
{
    public class ResetPasswordRequestModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }
    }
}