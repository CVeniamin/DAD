using System.Collections.Generic;
using CommandLine;

namespace OGP.Server
{
    internal class ArgsOptions
    {
        [Option('i', "pid", Required = true)]
        public string ServerId { get; set; }

        [Option('c', "cluster", Required = true)]
        public string ClusterId { get; set; }

        [Option('u', "url", Required = true)]
        public string ServiceUrl { get; set; }

        [Option('t', "tick", DefaultValue = 20)]
        public int TickDuration { get; set; }

        [Option('p', "players", DefaultValue = 5)]
        public int NumPlayers { get; set; }

        [OptionList('g', "games", Separator = ',', DefaultValue = new[] { "Random" })]
        public IList<string> Games { get; set; }
    }
}