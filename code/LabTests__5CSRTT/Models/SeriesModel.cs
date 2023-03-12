using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Models
{
    public record SeriesModel
    {
        public int Id { get; set; }

        public string SeriesFileName { get; set; }

        public DateTime StartDate { get;set;}

        public DateTime? EndDate { get;set;} = null;

        public Dictionary<string, string> RunParameters { get; set; }

        public void SetFileNameFromAnalysisName(string analysisName)
        {
            string result = analysisName + "__" + Id.ToString() + ".csv";
            SeriesFileName = result;
        }
    }
}
