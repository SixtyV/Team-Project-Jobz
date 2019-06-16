using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobz.Models
{
    public class JobModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Display(Name = "Title")]
        [Required(ErrorMessage = "Title is mandatory.")]
        [StringLength(100, ErrorMessage = "Must be up to 255 characters")]
        public string Title { get; set; }

        [Display(Name = "Choose Job Category")]
        [Required(ErrorMessage = "Job Category is mandatory.")]
        public int CategoryId { get; set; }

        [Display(Name = "Choose Region")]
        [Required(ErrorMessage = "Region is mandatory.")]
        public int RegionId { get; set; }

        [Display(Name = "Open Job possitions")]
        [Required(ErrorMessage = "Open Job possitions is mandatory.")]
        [Range(1, 200,  ErrorMessage = "Please enter a value greater than 1 an lower than 200")]
        public int Openings { get; set; }

        [Display(Name = "Work Hours")]
        [Required(ErrorMessage = "Work Hours is mandatory.")]
        public int WorkHoursId { get; set; }

        [Display(Name = "Job Description")]
        [Required(ErrorMessage = "Job Description is mandatory.")]
        [StringLength(255, ErrorMessage = "Must be up to 255 characters")]
        public string Content { get; set; }

        public bool Active { get; set; }

        [NotMapped]
        public List<JobListModel> JobCategories { get; set; }

        [NotMapped]
        public List<JobListModel> JobRegions { get; set; }

        [NotMapped]
        public List<JobListModel> WorkHours { get; set; }
    }   
}