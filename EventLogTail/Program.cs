using System;
using System.Diagnostics;
using System.Linq;


namespace EventLogTail
{
    class Program
    {
        // logs to tail  (only supports administrative eventlogs) !
        //static private String[] logNames = { "Application", "System", "Security", "Windows Powershell" };
        static private String hostName = ".";
        static private ConsoleColor defaultCC = ConsoleColor.White;
        static private String searchString = null;

        static void Main(string[] args)
        {
           
            // search string specified ? 
            if (  args.Length > 0 )
            {
                searchString = args[0];
                WriteLineConsole(String.Format("search string [{0}]",searchString), ConsoleColor.Cyan);
            }

            foreach (string logName in EventLog.GetEventLogs().Select(x => x.LogDisplayName))
            {

                // attach to eventlog
                EventLog elog = new EventLog(logName, hostName);
                try
                {

                    WriteLineConsole(String.Format("Registering eventlog [{0}]", elog.LogDisplayName), ConsoleColor.DarkGreen);
                    elog.EntryWritten += new EntryWrittenEventHandler(MyOnEntryWritten);
                    elog.EnableRaisingEvents = true;
                }
                catch (Exception e)
                {
                    WriteLineConsole(e.Message, ConsoleColor.Red);
                }
            }

            // now wait... :)
            while (Console.Read() != 'q')
            {

            }
        }

        public static void MyOnEntryWritten(object source, EntryWrittenEventArgs e)
        {

            string message = e.Entry.Message;
            string timeStamp = e.Entry.TimeGenerated.ToString();
            string machineName = e.Entry.MachineName.ToString();
            string logSource = e.Entry.Source.ToString();
            string logName = EventLog.LogNameFromSourceName(logSource, hostName);
            string entryType = e.Entry.EntryType.ToString();
            string instanceId = e.Entry.InstanceId.ToString();

            // check if a searchstring was provided and if the message matches..
            Boolean showLine = false;
            // display if no searchstring was given
            if (searchString == null) { showLine = true; }
            else
            {

                if (message.Contains(searchString)) { showLine = true; }
                if (logSource.Contains(searchString)) { showLine = true; }
                if (logName.Contains(searchString)) { showLine = true; }
                if (timeStamp.Contains(searchString)) { showLine = true; }
                if (entryType.Contains(searchString)) { showLine = true; }
                if (instanceId.Contains(searchString)) { showLine = true; }

            }
            if ( showLine)
            {
                //Console.WriteLine("[{0}:{1}:{2}:{3}:{4} {5}]: {6}", timeStamp, machineName, logName, logSource, entryType, instanceId, message);
                string header = String.Format("[{0}:{1}:{2}:{3}:{4} {5}]:", timeStamp, machineName, logName, logSource, entryType, instanceId);
                WriteConsole(header, ConsoleColor.DarkGreen);
                WriteLineConsole(message);
            }
        }


        public static void WriteLineConsole(string line, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.WriteLine(line);
            Console.ResetColor();
        }
        public static void WriteLineConsole(string line)
        {
            WriteLineConsole(line, defaultCC);
        }

        public static void WriteConsole(string line, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.Write(line);
            Console.ResetColor();
        }
        public static void WriteConsole(string line)
        {
            WriteConsole(line, defaultCC);
        }


    }
}
