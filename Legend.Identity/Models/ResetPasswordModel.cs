using System;
using System.ComponentModel.DataAnnotations;

namespace Legend.Identity.Models
{
    public class ResetPasswordModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
