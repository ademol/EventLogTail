using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLogTail
{
    interface ILogEntryNormalized
    {
        string Message { get; set; }
        string TimeStamp { get; set; }
        string MachineName { get; set; }
        string LogSource { get; set; }
        string LogName { get; set; }
        string EntryType { get; set; }
        long InstanceId { get; set; }
    }
}
