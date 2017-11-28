using CommandLine;

namespace OGP.Server
{
    internal class ArgsOptions
    {
        [Option("pcs", Required = false, DefaultValue = 0)]
        public string PCS { get; set; }

        [Option('p', "pid", Required = true)]
        public string PID { get; set; }

        [Option('u', "PCS_URL", Required = true)]
        public string PCS_URL { get; set; }

        [Option('s', "Server_URL", Required = true)]
        public string Server_URL { get; set; }

        [Option('m', "MSEC_PER_ROUND", DefaultValue = 20)]
        public int TickDuration { get; set; }

        [Option('n', "NUM_PLAYERS", DefaultValue = 5)]
        public int NumPlayers { get; set; }
    }
}