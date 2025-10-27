using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.domain
{
    public class Patient
    {
        public int PatientID { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; } 


        public ICollection<CTScan> CTScans { get; set; } = new List<CTScan>();
    }
}
