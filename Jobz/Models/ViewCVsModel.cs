using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Jobz.Models
{
    public class ViewCVsModel
    {
        [Display(Name = "Job ID")]
        public int JobId { get; set; }

        public string Title { get; set; }
        public string Category { get; set; }
        public string Region { get; set; }

        [Display(Name = "Work Hours")]
        public string WorkHours { get; set; }

        public int Openings { get; set; }
        public bool Active { get; set; }
        public DateTime Created { get; set; }

        [Display(Name = "CV Sender")]
        public string UserName { get; set; }
        public int IndividualId { get; set; }
    }
}