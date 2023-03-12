using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Models
{
    [Serializable]
    public class AnalysisModel
    {
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModificationDate { get; set; }

        public int LastSeriesId { get; set; }

        public int ProtocolId { get; set; }

        public Dictionary<string, string> ProtocolParameters { get; set; }

        public List<SeriesModel> Series { get; set; }
    }
}
