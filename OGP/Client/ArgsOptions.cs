using CommandLine;
using System.Collections.Generic;

namespace OGP.Client
{
    internal class ArgsOptions
    {
        [Option]
        public bool? Pcs { get; set; }

        [Option('p', Required = true)]
        public string Pid { get; set; }

        [Option('c', Required = true)]
        public string ClientUrl { get; set; }

        [Option('m', DefaultValue = 200, Required = true)]
        public int TickDuration { get; set; }

        [Option('n', DefaultValue = 5, Required = true)]
        public int NumPlayers { get; set; }

        [Option('f')]
        public string TraceFile { get; set; }

        [OptionList('s')]
        public IList<string> ServerEndpoints { get; set; }
    }
}