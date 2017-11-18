using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace OGP.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Console.WriteLine("Started Server with PID: " + argsOptions.PID);
                ConnectServer(8086, typeof(Game), "GameObject", WellKnownObjectMode.Singleton);
                System.Console.WriteLine("<enter> para sair...");
                Console.ReadLine();
            }

            //// Load requested games
            //List<string> supportedGames = new List<string>();
            //foreach (string gameName in argsOptions.Games)
            //{
            //    if (Type.GetType("OGP.Server.Games." + gameName) != null)
            //    {
            //        supportedGames.Add(gameName);
            //    } else { 
            //        Console.WriteLine("Following game is not supported: {0}", gameName);
            //    }
            //}

            //if (supportedGames.Count == 0)
            //{
            //    Console.WriteLine("None of the selected games are supported. Shutting down.");
            //    return;
            //}

        }

        private static void ConnectServer(int port,  Type t, String objName, WellKnownObjectMode wellKnown)
        {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            ActivateRemoteObject(t, objName, wellKnown);
        }

        private static void ActivateRemoteObject(Type t, String objName, WellKnownObjectMode wellKnown)
        {
            RemotingConfiguration.RegisterWellKnownServiceType(
                t,
                objName,
                wellKnown);
        }
    }
}