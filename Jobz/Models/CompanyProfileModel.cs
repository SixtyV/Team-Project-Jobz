using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobz.Models
{
    public class CompanyProfileModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Display(Name = "Company Name")]
        [Required(ErrorMessage = "Company Name is mandatory.")]
        [StringLength(255, ErrorMessage = "Must be up to 255 characters")]              
        public string CompanyName { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Company Description is mandatory.")]
        [StringLength(500, ErrorMessage = "Must be up to 500 characters")]
        public string Description { get; set; }

        [Display(Name = "Adress")]
        [Required(ErrorMessage = "Company Description is mandatory.")]
        [StringLength(100, ErrorMessage = "Must be up to 100 characters")]
        public string Adress { get; set; }

        [Display(Name = "Adress Number")]
        [Required(ErrorMessage = "Adress Number is mandatory.")]
        [StringLength(5, ErrorMessage = "Must be up to 5 characters")]
        public string AdressNumber { get; set; }

        [Display(Name = "Postal Corridor")]
        [Required(ErrorMessage = "Postal Corridor is mandatory.")]
        [RegularExpression(@"^(\d{5})$", ErrorMessage = "Must be 5 digits")]
        public int PostalCorridor { get; set; }

        [Display(Name = "Official Site ")]
        [Required(ErrorMessage = "OfficialSite  is mandatory.")]
        [StringLength(100, ErrorMessage = "Must be up to 100 characters")]
        public string OfficialSite { get; set; }
    }
}