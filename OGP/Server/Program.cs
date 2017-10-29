using System;
using System.Collections.Generic;
using System.Threading;

namespace OGP.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Console.WriteLine("Required args missing");
                return;
            }

            // Load requested games
            List<string> supportedGames = new List<string>();
            foreach (string gameName in argsOptions.Games)
            {
                if (Type.GetType("OGP.Server.Games." + gameName) != null)
                {
                    supportedGames.Add(gameName);
                } else { 
                    Console.WriteLine("Following game is not supported: {0}", gameName);
                }
            }
            if (supportedGames.Count == 0)
            {
                Console.WriteLine("None of the selected games are supported. Shutting down.");
                return;
            }

            Uri baseUri = new Uri(argsOptions.ServiceUrl);

            ServerDefinition serverDefinition = new ServerDefinition
            {
                SupportedGames = supportedGames,
                TickDuration = argsOptions.TickDuration,
                NumPlayers = argsOptions.NumPlayers
            };

            ClusterManager clusterManager = new ClusterManager(argsOptions.ClusterId, argsOptions.ServerId, baseUri, serverDefinition);
            GameManager gameManager = new GameManager(clusterManager, serverDefinition);
            ClientManager clientManager = new ClientManager(gameManager, baseUri);
            
            gameManager.Start();
            clientManager.Listen();

            Console.Read();

            Console.WriteLine("Shutting down...");

            (new Thread(clusterManager.Exit)).Start();
            (new Thread(gameManager.Exit)).Start();
            (new Thread(clientManager.Exit)).Start();

            Thread.Sleep(5000);
            Environment.Exit(0);
        }
    }
}