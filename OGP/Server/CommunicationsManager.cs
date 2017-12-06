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

        private CommandQueue commandQueue;
        private Thread backgroundThread;
        private TcpChannel channel;

        public InManager(string Url, ActionHandler actionHandler, ChatHandler chatHandler, StateHandler stateHandler, bool server)
        {
            Uri uri = new Uri(Url);

            //if (server)
            //{
            //    try
            //    {
            //        TcpChannel channel = new TcpChannel(uri.Port);
            //        ChannelServices.RegisterChannel(channel, true);
            //    }
            //    catch (SocketException)
            //    {
            //        Console.WriteLine("Could not bind to port. Either already occupied or blocked by firewall. Exiting.", "CRITICAL");
            //        initError = true;
            //        return; // Uncomment for client after refractoring
            //    }
                
            //}

            RemotingEndpoint endpoint = new RemotingEndpoint(this);
            RemotingServices.Marshal(endpoint, uri.AbsolutePath.Substring(1));

            this.actionHandler = actionHandler;
            this.chatHandler = chatHandler;
            this.stateHandler = stateHandler;

            this.commandQueue = new CommandQueue();

            backgroundThread = new Thread(HandlerThread);
            backgroundThread.Start();

            Console.WriteLine("Server Registered at " + Url);
        }

        private void HandlerThread()
        {
            while (true)
            {
                if (!frozen)
                {
                    Command command;
                    while ((command = commandQueue.Dequeue()) != null)
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

        public bool GotError()
        {
            return initError;
        }

        internal void Enqueue(Command command)
        {
            commandQueue.Enqueue(command);
            backgroundThread.Interrupt();
        }

        public void PassCommandToHandler(Command command)
        {
            switch (command.Type)
            {
                case Type.Action:
                    if (actionHandler != null)
                    {
                        actionHandler.Process(command.Sender, command.Args);
                    }
                    break;

                case Type.Chat:
                    if (chatHandler != null)
                    {
                        chatHandler.Process(command.Sender, command.Args);
                    }
                    break;

                case Type.State:
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
            backgroundThread.Interrupt();
        }
    }

    public enum Type
    { Action, Chat, State };

    [Serializable]
    public class Command
    {
        public Type Type { get; set; }
        public object Args { get; set; }
        internal string Sender { get; set; }
        internal long InsertedTime { get; set; }
    }

    public class OutManager
    {
        public const string MASTER_SERVER = "master";

        private Dictionary<string, CommandQueue> outQueues;
        private HashSet<CommandQueue> activeQueues;
        private EndpointPool endpointPool;

        private List<string> serverList;
        private string masterServer;
        private string selfUrl;

        private Thread backgroundThread;


        public Uri GetMasterServer()
        {
            return new Uri(masterServer);
        }
        public OutManager(string selfUrl, List<string> serverList)
        {
            if (serverList.Count == 0)
            {
                serverList.Add(selfUrl);
            }

            this.outQueues = new Dictionary<string, CommandQueue>();
            this.activeQueues = new HashSet<CommandQueue>();
            this.endpointPool = new EndpointPool();

            this.serverList = serverList;
            masterServer = serverList[0];

            this.selfUrl = selfUrl;

            backgroundThread = new Thread(SenderThread);
            backgroundThread.Start();
        }

        public bool SendCommand(Command command, string destination)
        {

            string Url = null;
            if (destination == MASTER_SERVER)
            {
                Url = this.masterServer;
            }
            /*else if (!destination.StartsWith("tcp://"))
            {
                Url = this.endpointPool.ResolveName(destination);
            }*/

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

            backgroundThread.Interrupt();

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

        private void SenderThread()
        {
            while (true)
            {
                foreach (CommandQueue commandQueue in activeQueues)
                {
                    string Url = outQueues.FirstOrDefault(x => x.Value == commandQueue).Key;
                    new Thread(() =>
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
                    }).Start();
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

        private void ReportOffline(string Url)
        {
            if (Url.Equals(masterServer))
            {
                FallbackToSlave();
            }
            else
            {
                // Remove clients if clients
            }
        }

        private void FallbackToSlave()
        {
            this.serverList.Sort();
            masterServer = serverList[0];
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