using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabTests__5CSRTT.Middleware
{
    public class StringEventLoggerSink : ILogEventSink
    {
        public delegate void LogMessageDelegate(string message);
        public event LogMessageDelegate LogMessageEvent;

        readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");

        public StringEventLoggerSink(LogMessageDelegate logMessageDelegate)
        {
            LogMessageEvent += logMessageDelegate;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            var renderSpace = new StringWriter();
            _textFormatter.Format(logEvent, renderSpace);

            LogMessageEvent?.Invoke(renderSpace.ToString() + "\r\n");
        }
    }
}
