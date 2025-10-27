using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.domain
{
    public class CTScan
    {
        public int ScanID { get; set; } 
        public int? PatientID { get; set; } 
        public string FileCode { get; set; }
        public string RelativePath { get; set; }
        public DateTime ScanDate { get; set; } = DateTime.Now;


        public Patient Patient { get; set; }

        public ICollection<AIModelOutput> AIOutputs { get; set; } = new List<AIModelOutput>();
    }
}
