using CommandLine;
using System.Collections.Generic;

namespace OGP.Server
{
    internal class ArgsOptions
    {
        [Option('d')]
        public bool Pcs { get; set; }

        [Option('p', Required = true)]
        public string Pid { get; set; }

        [Option('u', Required = true)]
        public string ServerUrl { get; set; }

        [Option('m', DefaultValue = 20)]
        public int TickDuration { get; set; }

        [Option('n', DefaultValue = 5)]
        public int NumPlayers { get; set; }

        [OptionList('s', Required = false)]
        public IList<string> ServerEndpoints { get; set; }
    }
}