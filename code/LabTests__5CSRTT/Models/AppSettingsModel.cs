using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Models
{
    public class AppSettingsModel
    {
        public string log_directory { get; set; } = "log";

        public string log_file_name { get; set; } = "log_{date}.log";

        public string log_level { get; set; } = "information";

        public string data_directory { get; set; } = "data";

        public string series_directory { get; set; } = "data";

        public string default_analysis_name { get; set; } = "тест #{index}";

        public string report_csv_separator { get; set; } = ";";

        public AppSettingsModel(IConfigurationRoot config)
        {
            Set(config);
        }

        public void Set(IConfigurationRoot config)
        {
            var props = this.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in props)
            {
                var value = config[prop.Name];

                if (value != null)
                {
                    prop.SetValue(this, value);
                }
            }
        }
    }
}
