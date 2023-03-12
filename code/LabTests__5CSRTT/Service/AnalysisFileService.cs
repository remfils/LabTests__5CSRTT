using LabTests__5CSRTT.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Service
{
    public class AnalysisFileService : IDisposable
    {
        private string _dataDirectory;

        public AnalysisFileService(AppSettingsModel config)
        {
            _dataDirectory = config.data_directory;
        }

        public bool SaveAnalysis(AnalysisModel analysis, bool allowOverride = false)
        {
            bool isOk = true;

            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                }

                string analysisFileName = analysis.Name + ".json";
                string analysisSavePath = this.GetFilePath(analysisFileName);
                if (allowOverride || !File.Exists(analysisSavePath))
                {
                    string json = JsonConvert.SerializeObject(analysis, Formatting.Indented);
                    File.WriteAllText(analysisSavePath, json);
                }
                else
                {
                    throw new Exception("File already exists");
                }
            }
            catch (Exception e)
            {
                Log.Error($"When writing file error occured: {e.Message}");
                isOk = false;
            }            

            return isOk;
        }

        public string GetFilePath(string fileName)
        {
            var result = Path.Join(_dataDirectory, fileName);
            return result;
        }

        public List<string> ReadAnalysesOrderedFileNames()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            var result = (new DirectoryInfo(_dataDirectory))
                .GetFiles()
                .Where(x => x.Extension == ".json")
                .OrderByDescending(x => x.LastWriteTime)
                .Select(x => Path.GetFileNameWithoutExtension(x.Name))
                .ToList();

            return result;
        }

        public void Dispose()
        {
            
        }
    }
}
