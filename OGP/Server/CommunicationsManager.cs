using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace OGP.Server
{
    internal class RemotingEndpoint : MarshalByRefObject
    {
        private InManager connectionManager;

        public RemotingEndpoint(InManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public void Request(Command command)
        {
            connectionManager.Enqueue(command);
        }
    }

    public class InManager
    {
        private bool initError = false;
        private bool frozen = false;

        private ActionHandler actionHandler;
        private ChatHandler chatHandler;
        private StateHandler stateHandler;

        private CommandQueue incomingCommandQueue;
        private Thread handlerThread;
        private TcpChannel channel;

        public InManager(string Url, ActionHandler actionHandler, ChatHandler chatHandler, StateHandler stateHandler, bool server)
        {
            Uri uri = new Uri(Url);
            
            try
            {
                TcpChannel channel = new TcpChannel(uri.Port);
                ChannelServices.RegisterChannel(channel, true);
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not bind to port. Either already occupied or blocked by firewall. Exiting.", "CRITICAL"); // TODO: Remove?
                throw new Exception("Socket not available");
            }

            RemotingEndpoint endpoint = new RemotingEndpoint(this);
            RemotingServices.Marshal(endpoint, uri.AbsolutePath.Substring(1));

            this.actionHandler = actionHandler;
            this.chatHandler = chatHandler;
            this.stateHandler = stateHandler;

            this.incomingCommandQueue = new CommandQueue();

            handlerThread = new Thread(ProcessIncomingMessages);
            handlerThread.Start();

            Console.WriteLine("Server Registered at " + Url);
        }

        private void ProcessIncomingMessages()
        {
            while (true)
            {
                if (!frozen)
                {
                    Command command;
                    while ((command = incomingCommandQueue.Dequeue()) != null)
                    {
                        PassCommandToHandler(command);

                        // Check if we got frozen in the mean time
                        if (frozen)
                        {
                            break;
                        }
                    }
                }

                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException) { }
            }
        }

        internal void Enqueue(Command command)
        {
            incomingCommandQueue.Enqueue(command);
            handlerThread.Interrupt();
        }

        public void PassCommandToHandler(Command command)
        {
            switch (command.Type)
            {
                case CommandType.Action:
                    if (actionHandler != null)
                    {
                        actionHandler.Process(command.Sender, command.Args);
                    }
                    break;

                case CommandType.Chat:
                    if (chatHandler != null)
                    {
                        chatHandler.Process(command.Sender, command.Args);
                    }
                    break;

                case CommandType.State:
                    if (stateHandler != null) {
                        stateHandler.Process(command.Sender, command.Args);
                    }
                    break;
            }
        }

        internal void Freeze()
        {
            frozen = true;
        }

        internal void Unfreeze()
        {
            frozen = false;
            handlerThread.Interrupt();
        }
    }

    public enum CommandType
    { Action, Chat, State };

    [Serializable]
    public class Command
    {
        public CommandType Type { get; set; }
        public object Args { get; set; }
        internal string Sender { get; set; }
        internal long InsertedTime { get; set; }
    }

    public class OutManager
    {
        public const string MASTER_SERVER = "master";
        public const string CLIENT_BROADCAST = "cbcast";

        private Dictionary<string, CommandQueue> outQueues;
        private HashSet<CommandQueue> activeQueues;
        private EndpointPool endpointPool;

        private List<string> serverList;
        private string masterServer;
        private string selfUrl;

        private GameState gameState;
        private Thread dispatchThread;
        
        public OutManager(string selfUrl, List<string> serverList, GameState gameState)
        {
            if (serverList.Count == 0)
            {
                serverList.Add(selfUrl);
            }

            this.outQueues = new Dictionary<string, CommandQueue>();
            this.activeQueues = new HashSet<CommandQueue>();
            this.endpointPool = new EndpointPool();

            this.gameState = gameState;

            this.serverList = serverList;
            masterServer = serverList[0];

            this.selfUrl = selfUrl;

            dispatchThread = new Thread(ProcessOutgoingCommands);
            dispatchThread.Start();
        }

        public bool SendCommand(Command command, string destination)
        {

            string Url = null;
            if (destination == MASTER_SERVER)
            {
                Url = this.masterServer;
            }
            else if (destination == CLIENT_BROADCAST)
            {
                foreach (Player player in gameState.Players)
                {
                    SendCommand(command, player.Url);
                }
                return true;
            }

            // Fallback to passed destination
            if (Url == null || Url.Length == 0)
            {
                Url = destination;
            }

            // If still nothing - fail
            if (Url.Length == 0)
            {
                throw new Exception("Message destination not set");
            }

            if (!outQueues.TryGetValue(Url, out CommandQueue commandQueue))
            {
                commandQueue = new CommandQueue();
                commandQueue.Enqueue(command);
                outQueues.Add(Url, commandQueue);
            }

            commandQueue.Enqueue(command);
            activeQueues.Add(commandQueue);

            dispatchThread.Interrupt();

            return true;
        }

        public void SetDelay(string dstUrl, int time)
        {
            if (outQueues.TryGetValue(dstUrl, out CommandQueue commandQueue))
            {
                commandQueue.SetDelay(time);
            }
        }

        public void UpdateServerList(List<string> serverList)
        {
            this.serverList = serverList;
            // Check if Master is still present
            if (!this.serverList.Exists(x => string.Equals(x, this.masterServer, StringComparison.OrdinalIgnoreCase)))
            {
                FallbackToSlave();
            }
        }

        private void ProcessOutgoingCommands()
        {
            while (true)
            {
                foreach (CommandQueue commandQueue in activeQueues)
                {
                    string Url = outQueues.FirstOrDefault(x => x.Value == commandQueue).Key;
                    new Thread(() => EmitOutgoingCommands(commandQueue, Url)).Start();
                }
                activeQueues.Clear();

                try
                {
                    Thread.Sleep(Timeout.Infinite);
                } catch (ThreadInterruptedException)
                {
                }
            }
        }

        private void EmitOutgoingCommands(CommandQueue commandQueue, string Url)
        {
            Command command = commandQueue.Dequeue();
            if (command != null)
            {
                command.Sender = this.selfUrl;
                RemotingEndpoint endpoint = endpointPool.GetByUrl(Url);

                try
                {
                    endpoint.Request(command);
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is SocketException)
                    {
                        ReportOffline(Url);
                    }
                }

            }
        }

        private void ReportOffline(string Url)
        {
            if (Url.Equals(masterServer))
            {
                FallbackToSlave();
            }
            else
            {
                // Remove client(s) and server(s) at that Url from local state
                gameState.Players.RemoveAll(player => player.Url == Url);
                gameState.Servers.RemoveAll(server => server.Url == Url);
            }
        }

        private void FallbackToSlave()
        {
            //while (gameState.Servers.Count > 0)
            //{
                //Server server = gameState.Servers[0];
                
            //}
            
            // if (server.Url == )
            // this.serverList.Sort();
            // masterServer = serverList[0];
        }
    }

    internal class CommandQueue
    {
        private int delay = 0;

        private Queue<Command> queue;

        public CommandQueue()
        {
            queue = new Queue<Command>();
        }

        internal void SetDelay(int delay)
        {
            throw new NotImplementedException();
        }

        internal void Enqueue(Command command)
        {
            command.InsertedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            queue.Enqueue(command);
        }

        internal Command Dequeue()
        {
            Command command = null;
            try
            {
                command = queue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            long pendingDelay = -1 ;
            if (command != null)
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                pendingDelay = currentTime - command.InsertedTime - delay;
            }

            if (pendingDelay < 0)
            {
                try
                {
                    Thread.Sleep(-(int)pendingDelay);
                } catch (ThreadInterruptedException) { }
            }

            return command;
        }
    }

    internal class EndpointPool
    {
        private Dictionary<string, RemotingEndpoint> endpoints;
        private TcpChannel channel;

        public EndpointPool()
        {
            endpoints = new Dictionary<string, RemotingEndpoint>();

            //channel = new TcpChannel();
            //ChannelServices.RegisterChannel(channel, true);
        }

        public RemotingEndpoint GetByUrl(string Url)
        {
            if (endpoints.TryGetValue(Url, out RemotingEndpoint endpoint))
            {
                return endpoint;
            }
            
            endpoint = (RemotingEndpoint)Activator.GetObject(typeof(RemotingEndpoint), Url);
            endpoints.Add(Url, endpoint);

            return endpoint;
        }
    }
}