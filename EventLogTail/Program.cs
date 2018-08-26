using CommandLine;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace EventLogTail
{
    class Program
    {
        static private String hostName = ".";
        private static readonly ConsoleColor defaultCC = ConsoleColor.White;
        static private String searchString = null;
        static private bool SearchExistingLogEntries = false;
        static private bool ShowDetail = false;
        static private Nullable<int> instanceId = null;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                            .WithParsed<Options>(o =>
                            {
                                if (o.SearchString?.Length > 0)
                                {
                                    searchString = o.SearchString;
                                    WriteLineConsole(String.Format("#search string [{0}]", searchString), ConsoleColor.Cyan);
                                }

                                SearchExistingLogEntries = o.ExistingLogEntries;
                                if (SearchExistingLogEntries) { WriteLineConsole(String.Format("#Displaying existing entries"), ConsoleColor.Cyan); }

                                ShowDetail = o.Detail;
                                if (ShowDetail) { WriteLineConsole(String.Format("#Detail log entry"), ConsoleColor.Cyan); }

                                if ( o.instanceId.HasValue )
                                {
                                    instanceId = o.instanceId;
                                }
                                if ( o.HostName?.Length > 0)
                                {
                                    hostName = o.HostName;
                                }

                            });

        if ( SearchExistingLogEntries )
            {
                DoExistingLogEntries();
            } else
            {
                DoTail();
            }
        }

        public static void DoTail()
        {
            foreach (string logName in EventLog.GetEventLogs().Select(x => x.Log))
            {
                // attach to eventlog
                EventLog elog = new EventLog(logName, hostName);
                try
                {
                    WriteLineConsole(String.Format("Registering eventlog [{0}]", logName), ConsoleColor.DarkGreen);
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

        public static void DoExistingLogEntries()
        {
            foreach (string logName in EventLog.GetEventLogs().Select(x => x.Log))
            {
                WriteLineConsole($"#parsing {logName}", ConsoleColor.Blue);
                EventLog elog = new EventLog(logName, hostName);

                var entries = elog.Entries;
                foreach (EventLogEntry entry in entries)
                {
                    ILogEntryNormalized entryNormalized = new ExistingLogEntry(entry);

                    if (MatchEntry(entryNormalized))
                    {
                        DisplayEntry(entryNormalized);
                    }
                }
            }
            WriteLineConsole("#parsing done", ConsoleColor.Blue);
            Console.ReadLine();
        }


        public static void MyOnEntryWritten(object source, EntryWrittenEventArgs e)
        {
            ILogEntryNormalized entryNormalized = new TailLogEntry(e);
            if ( MatchEntry(entryNormalized) ) { 
                DisplayEntry(entryNormalized);
            }
        }


        private static void DisplayEntry(ILogEntryNormalized e)
        {
            string header = String.Format("[{0}:{1}:{2}:{3}:{4} {5}]:"
                , e.TimeStamp, e.MachineName, e.LogName, e.LogSource, e.EntryType, e.InstanceId);
            WriteConsole(header, ConsoleColor.DarkGreen);

            if (ShowDetail)
            {
                 WriteLineConsole(e.Message);
            } else
            {
                string firstLine = Regex.Match(e.Message, @"^([^\n]+)")?.Value;
                WriteLineConsole(firstLine);
            }
 
        }

        private static bool MatchEntry(ILogEntryNormalized e)
        {
            Boolean lineMatched = false;
            Boolean lineMatchedSearchCriteria = false;
            Boolean lineMatchedEventCriteria = false;

            if ( searchString?.Length > 0 ) {
                lineMatchedSearchCriteria = MatchEntry(e, searchString);
            } else
            {
                // no limiting Search criteria
                lineMatchedSearchCriteria = true;  
            }

            if (instanceId.HasValue) {
                lineMatchedEventCriteria = MatchEntry(e, instanceId.Value);
            } else
            {
                // no limiting eventID criteria
                lineMatchedEventCriteria = true;
            }

            lineMatched = lineMatchedEventCriteria && lineMatchedSearchCriteria;

            return lineMatched;
        }


        private static bool MatchEntry(ILogEntryNormalized e, string matchString)
        {
            Boolean lineMatched = false;

            if (e.Message.Contains(matchString, StringComparison.OrdinalIgnoreCase)) { lineMatched = true; }
            if (e.LogSource.Contains(matchString, StringComparison.OrdinalIgnoreCase)) { lineMatched = true; }
            if (e.LogName.Contains(matchString, StringComparison.OrdinalIgnoreCase)) { lineMatched = true; }
            if (e.TimeStamp.Contains(matchString, StringComparison.OrdinalIgnoreCase)) { lineMatched = true; }
            if (e.EntryType.Contains(matchString, StringComparison.OrdinalIgnoreCase)) { lineMatched = true; }          
            return lineMatched;
        }

        private static bool MatchEntry(ILogEntryNormalized e, int matchInstanceID)
        {
            Boolean lineMatched = false;
            if (e.InstanceId == matchInstanceID) { lineMatched = true; }
            return lineMatched;
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
