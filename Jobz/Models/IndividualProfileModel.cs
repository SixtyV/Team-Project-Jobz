using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobz.Models
{
    public class IndividualProfileModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "User Name is mandatory.")]
        [StringLength(255, ErrorMessage = "Must be up to 255 characters")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is mandatory.")]
        [StringLength(255, ErrorMessage = "Must be up to 255 characters")]
        public string LastName { get; set; }
    }
}