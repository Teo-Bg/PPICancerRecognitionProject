using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.domain
{
    public class AIModelOutput
    {
        public int OutputID { get; set; }
        public int ScanID { get; set; }
        public string FileCode { get; set; }
        public string RelativePath { get; set; }
        public DateTime GenerationDate { get; set; } = DateTime.Now;

        public string PredictedTypes { get; set; }
        public string PredictedClass { get; set; }
        public string TypeProbabilities { get; set; }
        public string ClassProbabilities { get; set; }

        public CTScan CTScan { get; set; }


    }
}
