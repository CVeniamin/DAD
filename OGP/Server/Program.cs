using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGP.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions)) {
                // Args received, ready to launch
                // argsOptions.ServiceURL
            }
        }
    }
}
