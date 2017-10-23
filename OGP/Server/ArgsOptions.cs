using System.Collections.Generic;
using CommandLine;

namespace OGP.Server
{
    internal class ArgsOptions
    {
        [Option("url", Required = true)]
        public string ServiceURL { get; set; }
        
        [Option("tick", DefaultValue = 20)]
        public int TickDuration { get; set; }

        [Option("players", DefaultValue = 5)]
        public int NumPlayers { get; set; }
    }
}