using System;
using System.Collections.Generic;

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
            List<Game> offeredGames = new List<Game>();
            foreach (string gameName in argsOptions.Games)
            {
                try
                {
                    Type gameType = Type.GetType("OGP.Server.Games." + gameName);
                    offeredGames.Add((Game)Activator.CreateInstance(gameType, new object[] { argsOptions.TickDuration }));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Picked game is not avaialble: {0} [{1}]", gameName, e.Message);
                    continue;
                }
            }
            if (offeredGames.Count == 0)
            {
                Console.WriteLine("None of the selected games are avaialble");
                return;
            }

            Uri serviceUri = new Uri(argsOptions.ServiceUrl);

            ClusterManager clusterManager = new ClusterManager(argsOptions.ClusterId, argsOptions.ServerId, serviceUri);
            GameManager gameManager = new GameManager(clusterManager, offeredGames, argsOptions.TickDuration, argsOptions.NumPlayers);
            ClientManager clientManager = new ClientManager(gameManager, argsOptions.ServiceUrl);

            gameManager.start();
            clientManager.listen();

            Console.Read();
        }
    }
}