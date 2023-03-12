using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Service
{
    public class SimpleStatReportService
    {
        public string _csvSeparator = ",";

        public SimpleStatReportService(AppSettingsModel settings)
        {
            _csvSeparator = settings.report_csv_separator;
        }

        public void CalculateAndSaveReportToFile(int protocolId, Dictionary<int, EventRecordModel> data, string filename)
        {
            var eventsToCount = new List<string>();
            var trialStatEventCodes = new List<string>();

            switch (protocolId)
            {
                case AppConstants.PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__HOUSE_LIGHT:
                case AppConstants.PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__CUE_LIGHT:
                    eventsToCount.Add(HabitationMagazineTrainingProtocol__house_light.PROTOCOL_EVENT__CORRECT_POKE_START);
                    eventsToCount.Add(HabitationMagazineTrainingProtocol__house_light.PROTOCOL_EVENT__CORRECT_POKE_END);
                    eventsToCount.Add(HabitationMagazineTrainingProtocol__house_light.PROTOCOL_EVENT__WRONG_POKE_START);
                    eventsToCount.Add(HabitationMagazineTrainingProtocol__house_light.PROTOCOL_EVENT__WRONG_POKE_END);

                    trialStatEventCodes.Add(HabitationMagazineTrainingProtocol__house_light.PROTOCOL_EVENT__CORRECT_POKE_START);
                    break;
                case AppConstants.PROTOCOL_TYPE__FIXED_INTERVAL:
                    eventsToCount.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__WRONG_POKE_START);
                    eventsToCount.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__REINFORCED_POKE_START);
                    eventsToCount.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__NOT_REINFORCED_POKE_START);
                    eventsToCount.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__WRONG_POKE_END);
                    eventsToCount.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__REINFORCED_POKE_END);
                    eventsToCount.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__NOT_REINFORCED_POKE_END);

                    trialStatEventCodes.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__REINFORCED_POKE_START);
                    trialStatEventCodes.Add(FixedIntervalTrainingProtocol.PROTOCOL_EVENT__NOT_REINFORCED_POKE_START);
                    break;
                case AppConstants.PROTOCOL_TYPE__PEAK_INTERVAL:
                    eventsToCount.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__WRONG_POKE_START);
                    eventsToCount.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__REINFORCED_POKE_START);
                    eventsToCount.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__NOT_REINFORCED_POKE_START);
                    eventsToCount.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__WRONG_POKE_END);
                    eventsToCount.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__REINFORCED_POKE_END);
                    eventsToCount.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__NOT_REINFORCED_POKE_END);

                    trialStatEventCodes.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__REINFORCED_POKE_START);
                    trialStatEventCodes.Add(PeakIntervalTrainingProtocol.PROTOCOL_EVENT__NOT_REINFORCED_POKE_START);
                    break;
            }

            int trialCounter = 0;
            var trialEventCounter = new Dictionary<string, int>();
            var resultEventCount = new Dictionary<int, Dictionary<string, int>>();

            foreach (var keyValue in data)
            {
                var model = keyValue.Value;

                switch (model.EventType)
                {
                    case AppConstants.PROTOCOL_EVENT__TRIAL_START:
                        trialCounter++;

                        trialEventCounter = new Dictionary<string, int>();
                        foreach (var eventCode in eventsToCount)
                        {
                            trialEventCounter[eventCode] = 0;
                        }

                        break;
                    case AppConstants.PROTOCOL_EVENT__TRIAL_END:
                        resultEventCount[trialCounter] = trialEventCounter.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        break;
                    default:
                        if (eventsToCount.Contains(model.EventType))
                        {
                            trialEventCounter[model.EventType]++;
                        }
                        break;
                }
            }

            using (var fileStream = File.Create(filename))
            {
                using (var stream = new StreamWriter(fileStream))
                {
                    foreach (var eventCode in trialStatEventCodes)
                    {
                        stream.WriteLine("Trial code report: " + eventCode);

                        var resultDict = new Dictionary<int, int>();

                        foreach (var reportStatItem in resultEventCount.Values)
                        {
                            foreach (var eventStat in reportStatItem)
                            {
                                if (eventCode == eventStat.Key)
                                {
                                    if (resultDict.ContainsKey(eventStat.Value))
                                    {
                                        resultDict[eventStat.Value]++;
                                    }
                                    else
                                    {
                                        resultDict[eventStat.Value] = 1;
                                    }
                                }
                            }
                        }

                        stream.WriteLine("count events per trial" + _csvSeparator + "count trial");
                        foreach (var resultItem in resultDict.OrderBy(x => x.Key))
                        {
                            stream.WriteLine(resultItem.Key.ToString() + _csvSeparator + resultItem.Value.ToString());
                        }
                    }

                    stream.WriteLine();

                    stream.WriteLine("Trial statistic");

                    var headerString = "trial index" + _csvSeparator;
                    foreach (var eventCode in eventsToCount)
                    {
                        headerString += eventCode + _csvSeparator;
                    }
                    stream.WriteLine(headerString);

                    foreach (var reportStatItem in resultEventCount)
                    {
                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append("Trial #" + reportStatItem.Key + _csvSeparator);

                        foreach (var eventCode in eventsToCount)
                        {
                            stringBuilder.Append(reportStatItem.Value[eventCode].ToString() + _csvSeparator);
                        }

                        stream.WriteLine(stringBuilder.ToString());
                    }
                }
            }
        }
    }
}
