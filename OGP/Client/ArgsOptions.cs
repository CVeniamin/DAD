using CommandLine;
using System.Collections.Generic;

namespace OGP.Client
{
    internal class ArgsOptions
    {
        [Option('p', "PID", Required = true, DefaultValue = "1")]
        public string PID { get; set; }
        
        [Option('c', "Client_URL", Required = true)]
        public string Client_URL { get; set; }

        [Option('m', "MSEC_PER_OUND", DefaultValue = 200, Required = true)]
        public int TickDuration { get; set; }

        [Option('n', "NUM_PLAYERS", DefaultValue = 5, Required = true)]
        public int NumPlayers { get; set; }

        [Option('f', "trace", Required = false)]
        public string TraceFile { get; set; }

        [OptionList('s', "servers", Required = false)]
        public IList<string> ServerEndpoints { get; set; }
    }
}