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
                Console.WriteLine(argsOptions.Pcs);

                if (argsOptions.Pcs != null)
                {
                    Console.SetOut(new SuppressedWriter());
                }

                Console.WriteLine("Started Server with PID: " + argsOptions.Pid);
                // change usingSingleton to false to use Marshal activation
                Uri uri = new Uri(argsOptions.ServerUrl);
                TcpChannel channel = new TcpChannel(uri.Port);
                ChannelServices.RegisterChannel(channel, true);

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
            }
        }

        private static void WaitForPlayers(ArgsOptions argsOptions)
        {
            while (chatManager.getClients().Count < argsOptions.NumPlayers)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
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