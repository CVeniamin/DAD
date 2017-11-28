using OGP.PCS;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace OGP.PCS
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Port number required");
                return 1;
            }

            int port = Int32.Parse(args[0]);
            
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            PcsManager manager = new PcsManager();
            RemotingServices.Marshal(manager, "PCS");
            
            Console.WriteLine("PCS listening on port " + port);
            Console.ReadLine();

            return 0;
        }
    }
}