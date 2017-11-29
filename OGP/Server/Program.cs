using Sprache;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace OGP.Server
{
    internal class Program
    {
        //private GameService gameService;
        private static ChatManager chatManager;

        private static void RegisterServices()
        {
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(GameService),
                "GameService",
                WellKnownObjectMode.Singleton);

            //ChatService chatService = new ChatService();
            //RemotingServices.Marshal(chatService, "ChatService");

            chatManager = new ChatManager();
            RemotingServices.Marshal(chatManager, "ChatManager");

            //RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(ChatServerServices), "ChatServer",
            //    WellKnownObjectMode.Singleton);
        }

        private static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                if (argsOptions.Pcs == true)
                {
                    Console.WriteLine("Suppressing output");
                    Console.SetOut(new SuppressedWriter());
                }

                Console.WriteLine("Started Server with PID: " + argsOptions.Pid);
                // change usingSingleton to false to use Marshal activation
                Uri uri = new Uri(argsOptions.ServerUrl);

                try
                {
                    TcpChannel channel = new TcpChannel(uri.Port);
                    ChannelServices.RegisterChannel(channel, true);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Could not bind to port. Either already occupied or blocked by firewall. Exiting.", "CRITICAL");
                    return;
                }

                RegisterServices();
                Console.WriteLine("Server Registered at " + argsOptions.ServerUrl);
                Console.WriteLine("Server Port at " + uri.Port);
                Console.WriteLine("Waiting for players to join...");

                Thread t = new Thread(() => WaitForPlayers(argsOptions));
                t.Start();

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
            while (chatManager.getClients().Count < argsOptions.NumPlayers)
            {
                Thread.Sleep(1000);
            }

            // TODO: start game here (in a new thread, so that process does not die)
        }

        private static void ActivateRemoteObject(Type t, String objName, WellKnownObjectMode wellKnown)
        {
            RemotingConfiguration.RegisterWellKnownServiceType(
                t,
                objName,
                wellKnown);
        }
    }

    internal class ChatManager : MarshalByRefObject, IChatManager
    {
        private List<IChatClient> clients;

        public ChatManager()
        {
            clients = new List<IChatClient>();
        }

        public List<IChatClient> getClients()
        {
            //TODO: server needs to push the list of clients for each client after all clients are connected
            return clients;
        }

        public IChatClient RegisterClient(string url)
        {
            Console.WriteLine("New client listening at " + url);
            IChatClient newClient = (IChatClient)Activator.GetObject(typeof(IChatClient), url);
            clients.Add(newClient);
            return newClient;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}