using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Legend.Admin.Models
{
    public class CreateUserViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Select Role")]
        public int Role { get; set; }
        [Required]
        public string ParkSeq { get; set; }

        [Required(ErrorMessage = "Operation is required")]
        [Display(Name = "Select Operations")]
        public IEnumerable<int> SelectedOperations { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> OperationList { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> RoleList { get; set; }

    }
} 