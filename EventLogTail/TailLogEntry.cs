using System.Diagnostics;
using System.Globalization;

namespace EventLogTail
{
    class TailLogEntry : ILogEntryNormalized
    {
        public string Message { get; set; }
        public string TimeStamp { get; set; }
        public string MachineName { get; set; }
        public string LogSource { get; set; }
        public string LogName { get; set; }
        public string EntryType { get; set; }
        public long InstanceId { get; set; }

        public TailLogEntry(EntryWrittenEventArgs e)
        {
            Message = e.Entry.Message;
            TimeStamp = e.Entry.TimeGenerated.ToString(CultureInfo.InvariantCulture);
            MachineName = e.Entry.MachineName;
            LogSource = e.Entry.Source;
            LogName = EventLog.LogNameFromSourceName(LogSource, MachineName);
            EntryType = e.Entry.EntryType.ToString();
            InstanceId = e.Entry.InstanceId;
        }
    }
}
