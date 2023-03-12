using LabTests__5CSRTT.Models;
using LabTests__5CSRTT.Protocols;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Service
{
    public class DataRepository
    {

        public static List<string> GetSerialPorts()
        {

            var result = new List<string>();
            foreach (string s in SerialPort.GetPortNames())
            {
                result.Add(s);
            }
            return result;
        }

        public static IProtocol_5CSRTT CreateProtocolById(int protocolId)
        {
            IProtocol_5CSRTT protocol = null;

            switch (protocolId)
            {
                case AppConstants.PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__CUE_LIGHT:
                    protocol = new HabitationMagazineTrainingProtocol__cue_light();
                    break;
                case AppConstants.PROTOCOL_TYPE__HABITATION_MAGAZINE_TRAINING__HOUSE_LIGHT:
                    protocol = new HabitationMagazineTrainingProtocol__house_light();
                    break;
                case AppConstants.PROTOCOL_TYPE__FIXED_INTERVAL:
                    protocol = new FixedIntervalTrainingProtocol();
                    break;
                case AppConstants.PROTOCOL_TYPE__PEAK_INTERVAL:
                    protocol = new PeakIntervalTrainingProtocol();
                    break;
                default:
                    break;
            }

            return protocol;
        }

        public static double MillisecondsToSeconds(long ms)
        {
            double result = (double)ms / 1000.0d;
            result = Math.Round(result, 2);
            return result;
        }

        public static string MillisecondsToFloatString(long ms)
        {
            string result = ((float)(ms / (float)1000)).ToString("0.00", CultureInfo.InvariantCulture);
            return result;
        }

        public static string InternalPercentToFloatString(int internalPercent)
        {
            string result = ((float)((float)internalPercent / 100.0f)).ToString("0.00", CultureInfo.InvariantCulture);
            return result;
        }

        public static bool TryGetIntValueFromDictionary(Dictionary<string, string> parameters, string key, out int value)
        {
            value = 0;
            bool result = true;
            string valueToParse = null;
            if (parameters.TryGetValue(key, out valueToParse))
            {
                if (Int32.TryParse(valueToParse.Trim(), out value))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public static bool TryGetFloatValueFromDictionary(Dictionary<string, string> parameters, string key, out float value)
        {
            value = 0;
            bool result = true;
            string valueToParse = null;
            if (parameters.TryGetValue(key, out valueToParse))
            {
                if (float.TryParse(valueToParse.Trim().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
