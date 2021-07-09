using System;
using System.Collections.Generic;
using System.Text;

namespace VeemedELBTokenEndPointProxy
{
    public class TokenPostBody
    {
        public string SerialNumber { get; set; }
        public string MRN { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public string PatientDOB { get; set; }
        public string PatientSex { get; set; }
        public string Reason { get; set; }
        public string ProviderId { get; set; }
    }
}
