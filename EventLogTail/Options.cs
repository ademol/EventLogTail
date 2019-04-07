using CommandLine;

namespace EventLogTail
{
    class Options
    {
        [Option('h', "hostname", Required = false, HelpText = "Hostname to get logs from, defaults to '.'")]
        public string HostName { get; set; }

        [Option('s', "searchString", Required = false, HelpText = "String (caseInsensitve) to search for")]
        public string SearchString { get; set; }

        [Option('i', "instanceId", Required = false, HelpText = "Limit result to this instanceId")]
        public int? InstanceId { get; set; }

        [Option('e', "existing", Default = false, HelpText = "Dont tail, but display existing entries")]
        public bool ExistingLogEntries { get; set; }

        [Option('d', "detail", Default = false, HelpText = "Display logentry details")]
        public bool Detail { get; set; }

    }
}
