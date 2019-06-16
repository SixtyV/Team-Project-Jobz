using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Jobz.Models
{
    public class UploadCVSpecificJobModel
    {
        [Display(Name = "Select File")]
        [Required(ErrorMessage = "A PDF file is mandatory.")]
        [DataType(DataType.Upload)]        
        public HttpPostedFileBase File { get; set; }

        public string DescriptiveFileName { get; set; }
        public int CompanyId { get; set; }
        public int JobId { get; set; }
        public int UserId { get; set; }
    }
}


