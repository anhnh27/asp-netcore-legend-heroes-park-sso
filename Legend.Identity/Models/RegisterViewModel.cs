using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Legend.Identity.Models
{
    public class RegisterViewModel
    {
        public Guid Id { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string Picture { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public int? PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public int? Nationality { get; set; }
        public string BirthDay { get; set; }
        public int? Gender { get; set; }
        public bool FromWeb { get; set; }
        public List<Country> Countries { get; set; }
        public List<Gender> Genders { get; set; }
    }
}
