using System;
namespace Legend.Identity.Models
{
    public class ResetPwdViewModel
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
