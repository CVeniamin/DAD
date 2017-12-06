using Sprache;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Threading;

namespace OGP.Server
{
    internal class Program
    {
        private static ChatManager chatManager;
        private static GameStateProxy gameProxy;
        private static GameState gameState;
        private static Game game;

        private static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                if (argsOptions.Pcs == true)
                {
                    Console.WriteLine("Suppressing output");
                    //Console.SetOut(new SuppressedWriter());
                }

                Console.WriteLine("Started Server with PID: " + argsOptions.Pid);
                
                gameState = new GameState();

                ActionHandler actionHandler = new ActionHandler(gameState);

                List<string> otherServersList;
                if (argsOptions.ServerEndpoints != null)
                {
                    otherServersList = (List<string>)argsOptions.ServerEndpoints;
                } else
                {
                    otherServersList = new List<string>();
                }

                OutManager outManager = new OutManager(argsOptions.ServerUrl, otherServersList);
                InManager connectionManager = new InManager(argsOptions.ServerUrl, actionHandler, null, null, true);

                actionHandler.SetOutManager(outManager);

                if (connectionManager.GotError())
                {
                    Console.WriteLine("Error initializing. Exiting.");
                    return;
                }

                new Thread(() =>
                {
                    while (true)
                    {
                        actionHandler.NotifyOfState();

                        Thread.Sleep(argsOptions.TickDuration);
                    }
                }).Start();

                chatManager = new ChatManager();
                RemotingServices.Marshal(chatManager, "ChatManager");

                Console.WriteLine("Waiting for players to join...");

                Thread waitChatClients = new Thread(() => WaitForPlayers(argsOptions));
                waitChatClients.Start();

                Thread waitGameClients = new Thread(() => StartGame(argsOptions));
                waitGameClients.Start();

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

                // Start listening for input
                while (true)
                {
                    var input = Console.ReadLine();

                    if (input == null || input.Trim() == "Quit")
                    {
                        Console.WriteLine("Exit triggered by input", "CRITICAL");
                        break;
                    }

                    ICommand cmd = CommandParser.Command.Parse(input);
                    cmd.Exec(null); // TODO: dependency injection to allow grabbing state and such
                }
            }
            else
            {
                Console.WriteLine("Missing required arguments");
            }
        }

        private static void WaitForPlayers(ArgsOptions argsOptions)
        {
            while (chatManager.GetClients().Count < argsOptions.NumPlayers)
            {
                Thread.Sleep(1000);
                Console.Write(".");
            }
            Console.WriteLine("Game STarted");
            chatManager.GameStarted = true;
            game.GameStarted = true;

            // TODO: start game here (in a new thread, so that process does not die)
        }

        private static void StartGame(ArgsOptions argsOptions)
        {
            game = new Game(gameState, argsOptions.TickDuration, argsOptions.NumPlayers, new Random().Next(1, 23));
            //gameState.Players = game.CreatePlayers();

            game.Init(41);

            //gameState.Walls = game.CreateWalls();
            //gameState.Coins = game.CreateCoins(41);
            //gameState.Ghosts = game.CreateGhosts();

            gameProxy = new GameStateProxy(gameState);
            RemotingServices.Marshal(gameProxy, "GameStateProxy");
        }
    }
}