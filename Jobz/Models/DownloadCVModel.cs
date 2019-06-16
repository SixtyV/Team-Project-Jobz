using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobz.Models
{
    public class DownloadCVModel
    {
        public byte[] FileContent { get; set; }
        public string Filename { get; set; }
        public string DescriptiveFilename { get; set; }
    }
}
