using System.Diagnostics;
using System.Globalization;

namespace EventLogTail
{
    class ExistingLogEntry : ILogEntryNormalized
    {
        public string Message { get ; set ; }
        public string TimeStamp { get; set; }
        public string MachineName { get; set; }
        public string LogSource { get; set; }
        public string LogName { get; set; }
        public string EntryType { get; set; }
        public long InstanceId { get; set; }

        public ExistingLogEntry(EventLogEntry e)
        {
            Message = e.Message;
            TimeStamp = e.TimeGenerated.ToString(CultureInfo.InvariantCulture);
            MachineName = e.MachineName;
            LogSource = e.Source;
            LogName = EventLog.LogNameFromSourceName(LogSource, MachineName);
            EntryType = e.EntryType.ToString();
            InstanceId = e.InstanceId;
        }

    }
}
