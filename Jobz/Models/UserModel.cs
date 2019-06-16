using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;
using System.ComponentModel.DataAnnotations.Schema;


namespace Jobz.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "User Name is mandatory.")]
        [StringLength(100, ErrorMessage = "Must be up to 100 characters")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "User Password is mandatory.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Must be between 8 and 100 characters.")]
        public string Password { get; set; }

        [NotMapped]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password is mandatory")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Must be between 8 and 100 characters")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Email is mandatory.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
        [StringLength(100, ErrorMessage = "Must be up to 100 characters")]
        public string Email { get; set; }

        [Display(Name = "User Role")]
        [Required(ErrorMessage = "User Role is mandatory.")]
        public UserRole Role { get; set; }

        [Display(Name = "Rights Granted")]
        public bool RightsApproved { get; set; }
    }

    public enum UserRole
    {
        Individual,
        Company,
        Admin      
    }
}