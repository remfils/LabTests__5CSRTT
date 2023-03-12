using LabTests__5CSRTT.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Service
{
    public class ClockService
    {
        Stopwatch _stopWatch;

        long firstEventMs = 0;

        public ClockService(AppSettingsModel config)
        {
                        
        }

        public void StartClock()
        {
            firstEventMs = 0;
            _stopWatch = Stopwatch.StartNew();
        }

        public void StopClock()
        {
            _stopWatch.Stop();
        }

        public void RegisterChange(out long totalPassed)
        {
            if (firstEventMs == 0)
            {
                firstEventMs = _stopWatch.ElapsedMilliseconds;
                totalPassed = 0;
            }
            else
            {
                long passed = _stopWatch.ElapsedMilliseconds - firstEventMs;
                totalPassed = passed;
            }
        }
    }
}
