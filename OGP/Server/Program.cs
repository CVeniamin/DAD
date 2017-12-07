using Sprache;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace OGP.Server
{
    internal class Program
    {
        private static ChatManager chatManager;
        private static GameStateProxy gameProxy;
        private static GameState gameState;
        private static Game game;

        private static OutManager outManager;
        private static StateHandler stateHandler;
        private static InManager connectionManager;
        private static ActionHandler actionHandler;

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

                Uri uri = new Uri(argsOptions.ServerUrl);

                TcpChannel channel = new TcpChannel(uri.Port);
                ChannelServices.RegisterChannel(channel, true);

                gameState = new GameState();

                chatManager = new ChatManager();
                RemotingServices.Marshal(chatManager, "ChatManager");

                Thread waitClients = new Thread(() => WaitForPlayers(argsOptions));
                waitClients.Start();

                Thread waitPlayers = new Thread(() => StartGame(argsOptions));
                waitPlayers.Start();

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
            Console.WriteLine("Waiting for players to join...");

            while (chatManager.ClientsEndpoints.Count < argsOptions.NumPlayers)
            {
                Thread.Sleep(500);
                Console.Write(".");
            }
            chatManager.GameStarted = true;

            // TODO: start game here (in a new thread, so that process does not die)
        }

        private static void StartGame(ArgsOptions argsOptions)
        {
            game = new Game(gameState, argsOptions.TickDuration, argsOptions.NumPlayers, new Random().Next(1, 23));

            gameProxy = new GameStateProxy(gameState);
            game.InitElements(41);

            RemotingServices.Marshal(gameProxy, "GameStateProxy");

            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(GameStateProxy),
            //"GameStateProxy", WellKnownObjectMode.Singleton);

            while (!chatManager.GameStarted)
            {
                Thread.Sleep(500);
                Console.Write(".");
            }

            Thread notifyState = new Thread(() => { SetupCommunication(game, argsOptions); NotifyState(argsOptions); });
            notifyState.Start();

            game.InitPlayers(chatManager.ClientsEndpoints);
            Console.WriteLine("\n Game started!");
            Thread.Sleep(1000);
            gameProxy.GameStarted = true;
        }

        private static void SetupCommunication(Game game, ArgsOptions argsOptions)
        {
            actionHandler = new ActionHandler(game);

            List<string> otherServersList;
            if (argsOptions.ServerEndpoints != null)
            {
                otherServersList = (List<string>)argsOptions.ServerEndpoints;
            }
            else
            {
                otherServersList = new List<string>();
            }

            outManager = new OutManager(argsOptions.ServerUrl, otherServersList);
            stateHandler = new StateHandler();
            connectionManager = new InManager(argsOptions.ServerUrl, actionHandler, null, stateHandler, true);

            actionHandler.SetOutManager(outManager);
            stateHandler.SetOutManager(outManager);

            if (connectionManager.GotError())
            {
                Console.WriteLine("Error initializing. Exiting.");
                return;
            }
        }

        public static void NotifyState(ArgsOptions argsOptions)
        {
            while (true)
            {
                actionHandler.NotifyOfState();

                Thread.Sleep(argsOptions.TickDuration);
            }
        }
    }
}