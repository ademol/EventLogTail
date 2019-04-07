using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace EventLogTail
{
    class Program
    {
        static private String _hostName = ".";
        private static readonly ConsoleColor defaultCC = ConsoleColor.White;
        static private String _searchString;
        static private bool _searchExistingLogEntries;
        static private bool _showDetail;
        static private int? _instanceId;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                            .WithParsed(o =>
                            {
                                if (o.SearchString?.Length > 0)
                                {
                                    _searchString = o.SearchString;
                                    WriteLineConsole(String.Format("#search string [{0}]", _searchString), ConsoleColor.Cyan);
                                }

                                _searchExistingLogEntries = o.ExistingLogEntries;
                                if (_searchExistingLogEntries) { WriteLineConsole("#Displaying existing entries", ConsoleColor.Cyan); }

                                _showDetail = o.Detail;
                                if (_showDetail) { WriteLineConsole("#Detail log entry", ConsoleColor.Cyan); }

                                if ( o.InstanceId.HasValue )
                                {
                                    _instanceId = o.InstanceId;
                                }
                                if ( o.HostName?.Length > 0)
                                {
                                    _hostName = o.HostName;
                                }

                            });

        if ( _searchExistingLogEntries )
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
                EventLog elog = new EventLog(logName, _hostName);
                try
                {
                    WriteLineConsole(String.Format("Registering eventlog [{0}]", logName), ConsoleColor.DarkGreen);
                    elog.EntryWritten += MyOnEntryWritten;
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
                EventLog elog = new EventLog(logName, _hostName);

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

            if (_showDetail)
            {
                 WriteLineConsole(e.Message);
            } else
            {
                string firstLine = Regex.Match(e.Message, @"^([^\n]+)").Value;
                WriteLineConsole(firstLine);
            }
 
        }

        private static bool MatchEntry(ILogEntryNormalized e)
        {
            Boolean lineMatched;
            Boolean lineMatchedSearchCriteria;
            Boolean lineMatchedEventCriteria;

            if ( _searchString?.Length > 0 ) {
                lineMatchedSearchCriteria = MatchEntry(e, _searchString);
            } else
            {
                // no limiting Search criteria
                lineMatchedSearchCriteria = true;  
            }

            if (_instanceId.HasValue) {
                lineMatchedEventCriteria = MatchEntry(e, _instanceId.Value);
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

        private static bool MatchEntry(ILogEntryNormalized e, int matchInstanceId)
        {
            bool lineMatched = e.InstanceId == matchInstanceId;
            return lineMatched;
        }

        private static void WriteLineConsole(string line, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.WriteLine(line);
            Console.ResetColor();
        }

        private static void WriteLineConsole(string line)
        {
            WriteLineConsole(line, defaultCC);
        }

        private static void WriteConsole(string line, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.Write(line);
            Console.ResetColor();
        }
    }
}
