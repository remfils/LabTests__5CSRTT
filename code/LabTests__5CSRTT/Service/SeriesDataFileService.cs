using LabTests__5CSRTT.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Service
{
    public class SeriesDataFileService : IDisposable
    {
        private FileStream? _fileStream;
        private string _directory;

        public SeriesDataFileService(AppSettingsModel config)
        {
            _directory = config.series_directory;

            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
        }

        public bool IsRunning => _fileStream != null;

        public void StartFileWrite(string filename, bool overrideFile = false)
        {
            string filePath = Path.Join(_directory, filename);
            if (overrideFile || !File.Exists(filePath))
            {
                _fileStream = File.OpenWrite(filePath);
            }
            else
            {
                _fileStream = null;
                Log.Error($"Error openining file [{filename}, file already exists]");
            }
        }

        public void StopFileWrite()
        {
            _fileStream?.Close();
        }

        public void WriteEventRecord(EventRecordModel eventRecord)
        {
            List<string> data = new List<string>();
            data.Add(eventRecord.EventType);
            data.Add(eventRecord.SecondsTrial.ToString("0.00", CultureInfo.InvariantCulture));
            data.Add(eventRecord.SecondsTotal.ToString("0.00", CultureInfo.InvariantCulture));
            data.Add(eventRecord.MillisecondsTotal.ToString());
            data.Add(eventRecord.Parameters.ToString());

            byte[] bytes = Encoding.UTF8.GetBytes(String.Join(",", data) + "\n");

            _fileStream?.Write(bytes, 0, bytes.Length);
        }

        public Dictionary<int, EventRecordModel> LoadAsDictionary(string seriesFileName)
        {
            Dictionary<int, EventRecordModel> result = new Dictionary<int, EventRecordModel>();

            string filePath = GetFileName(seriesFileName);
            if (File.Exists(filePath))
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                {
                    string? line;
                    int lineCounter = 0;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var data = line.Split(',');
                        if (data.Length == 4)
                        {
                            // TODO: remove deprecated
                            var model = new EventRecordModel();
                            model.EventType = data[0];

                            if (!double.TryParse(data[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out model.SecondsTotal))
                            {
                                Log.Warning("failed to parse seconds from file");
                            }

                            if (!long.TryParse(data[2], out model.MillisecondsTotal))
                            {
                                Log.Warning("failed to parse milliseconds from file");
                            }

                            model.Parameters = data[3];

                            result.Add(lineCounter++, model);
                        }
                        else if (data.Length == 5)
                        {
                            var model = new EventRecordModel();
                            model.EventType = data[0];

                            if (!double.TryParse(data[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out model.SecondsTrial))
                            {
                                Log.Warning("failed to parse seconds from file");
                            }

                            if (!double.TryParse(data[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out model.SecondsTotal))
                            {
                                Log.Warning("failed to parse seconds from file");
                            }

                            if (!long.TryParse(data[3], out model.MillisecondsTotal))
                            {
                                Log.Warning("failed to parse milliseconds from file");
                            }

                            model.Parameters = data[4];

                            result.Add(lineCounter++, model);
                        }
                    }
                }
            }
            else
            {
                Log.Error($"Failed opening file [{filePath}]");
            }

            return result;
        }

        internal string GetFileName(string seriesFileName)
        {
            string result = Path.Join(_directory, seriesFileName);
            return result;
        }

        public void Dispose()
        {
            StopFileWrite();
        }
    }
}
