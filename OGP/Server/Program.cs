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

                ActionHandler actionHandler = new ActionHandler();
                ChatHandler chatHandler = new ChatHandler();
                StateHandler stateHandler = new StateHandler();
                
                InManager connectionManager = new InManager(argsOptions.ServerUrl, actionHandler, chatHandler, stateHandler);

                if (connectionManager.GotError())
                {
                    Console.WriteLine("Error initializing. Exiting.");
                    return;
                }

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

            chatManager.GameStarted = true;

            // TODO: start game here (in a new thread, so that process does not die)
        }

        private static void StartGame(ArgsOptions argsOptions)
        {
            gameState = new GameState();
            Game game = new Game(argsOptions.TickDuration, argsOptions.NumPlayers, new Random().Next(1, 23));
            gameState.Ghosts = game.CreateGhosts();

            gameProxy = new GameStateProxy(gameState);
            RemotingServices.Marshal(gameProxy, "GameStateProxy");
        }
    }
}