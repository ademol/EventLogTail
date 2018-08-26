using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.Message = e.Entry.Message;
            this.TimeStamp = e.Entry.TimeGenerated.ToString();
            this.MachineName = e.Entry.MachineName.ToString();
            this.LogSource = e.Entry.Source.ToString();
            this.LogName = EventLog.LogNameFromSourceName(this.LogSource, this.MachineName);
            this.EntryType = e.Entry.EntryType.ToString();
            this.InstanceId = e.Entry.InstanceId;
        }
    }
}
