using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Models
{
    public record EventRecordModel
    {
        public long MillisecondsTotal = 0;

        public double SecondsTotal = 0.0d;

        public double SecondsTrial = 0.0d;

        public string EventType = "";

        public string Parameters = "";
    }
}
