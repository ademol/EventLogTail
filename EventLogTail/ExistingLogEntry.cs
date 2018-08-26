using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.Message = e.Message;
            this.TimeStamp = e.TimeGenerated.ToString();
            this.MachineName = e.MachineName.ToString();
            this.LogSource = e.Source.ToString();
            this.LogName = EventLog.LogNameFromSourceName(this.LogSource, this.MachineName);
            this.EntryType = e.EntryType.ToString();
            this.InstanceId = e.InstanceId;
        }

    }
}
