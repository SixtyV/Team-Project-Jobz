using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Jobz.Models
{
    public class UserLoginModel
    {
        [Display(Name = "User Name")]
        [Required(ErrorMessage = "A Username is mandatory")]
        [StringLength(100, ErrorMessage = "Must be up to 100 characters")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A Password is mandatory")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Must be between 8 and 100 characters.")]
        public string Password { get; set; }
    }
}