using CommandLine;

namespace OGP.Client
{
    internal class ArgsOptions
    {
        [Option("url", Required = true)]
        public string ServiceURL { get; set; }

        [Option("trace")]
        public string TraceFile { get; set; }

        [Option("tick", DefaultValue = 20)]
        public int TickDuration { get; set; }

        [Option("players", DefaultValue = 5)]
        public int NumPlayers { get; set; }
    }
}