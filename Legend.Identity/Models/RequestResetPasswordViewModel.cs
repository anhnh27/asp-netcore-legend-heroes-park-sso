using System;
using System.ComponentModel.DataAnnotations;

namespace Legend.Identity.Models
{
    public class RequestResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
