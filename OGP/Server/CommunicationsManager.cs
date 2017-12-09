using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Threading;
using OGP.Middleware;

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

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    public class InManager
    {
        private bool frozen = false;

        private ActionHandler actionHandler;
        private ChatHandler chatHandler;
        private StateHandler stateHandler;

        private CommandQueue incomingCommandQueue;
        private Thread handlerThread;

        private Object passLock = new Object();

        public InManager(string Url, ActionHandler actionHandler, ChatHandler chatHandler, StateHandler stateHandler)
        {
            Uri uri = new Uri(Url);

            RemotingEndpoint endpoint = new RemotingEndpoint(this);
            RemotingServices.Marshal(endpoint, uri.AbsolutePath.Substring(1));

            this.actionHandler = actionHandler;
            this.chatHandler = chatHandler;
            this.stateHandler = stateHandler;

            this.incomingCommandQueue = new CommandQueue();

            handlerThread = new Thread(ProcessIncomingMessages);
            handlerThread.Start();
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
                        lock (passLock)
                        {
                            try
                            {
                                PassCommandToHandler(command);
                            }
                            catch (ThreadInterruptedException)
                            {
                            }
                        }

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
                    if (stateHandler != null)
                    {
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
        
        private string masterServer;
        private string selfUrl;

        private GameState gameState;
        private Thread dispatchThread;
        private Object activeQueueLock = new Object();

        public OutManager(string selfUrl, string masterUrl, GameState gameState)
        {
            if (masterUrl == null)
            {
                masterUrl = selfUrl;
            }

            this.outQueues = new Dictionary<string, CommandQueue>();
            this.activeQueues = new HashSet<CommandQueue>();
            this.endpointPool = new EndpointPool();

            this.gameState = gameState;
            
            this.selfUrl = selfUrl;
            this.masterServer = masterUrl;

            dispatchThread = new Thread(ProcessOutgoingCommands);
            dispatchThread.Start();
        }

        public bool SendCommand(Command command, string destination)
        {
            string Url = null;
            if (destination == MASTER_SERVER)
            {
                Url = masterServer;
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
                outQueues.Add(Url, commandQueue);
            }

            commandQueue.Enqueue(command);

            lock (activeQueueLock)
            {
                activeQueues.Add(commandQueue);
            }

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

        public string GetMasterServer()
        {
            return masterServer;
        }
        
        private void ProcessOutgoingCommands()
        {
            while (true)
            {
                lock (activeQueueLock)
                {
                    foreach (CommandQueue commandQueue in activeQueues)
                    {
                        string Url = outQueues.FirstOrDefault(x => x.Value == commandQueue).Key;
                        new Thread(() => EmitOutgoingCommands(commandQueue, Url)).Start();
                    }
                    activeQueues.Clear();
                }

                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                }
            }
        }

        private void EmitOutgoingCommands(CommandQueue commandQueue, string Url)
        {
            Command command;
            while ((command = commandQueue.Dequeue()) != null)
            {
                command.Sender = this.selfUrl;
                DispatchOutgoingCommand(command, Url, 0);
            }
        }

        private void DispatchOutgoingCommand(Command command, string Url, int dispatchAttempt)
        {
            if (dispatchAttempt > 5)
            {
                return;
            }
            
            try
            {
                endpointPool.GetByUrl(Url).Request(command);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                {
                    Console.WriteLine("{0} detected as offline", Url);
                    if (Url.Equals(masterServer))
                    {
                        ReportOffline(Url);
                        // Attempt to redispatch to new master
                        DispatchOutgoingCommand(command, masterServer, dispatchAttempt++);
                    }
                    else
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
                gameState.Servers.RemoveAll(server => server.Url == masterServer);

                if (gameState.Servers.Count == 0)
                {
                    Console.WriteLine("No active servers. Nowhere to go. Exiting.");
                    Process.GetCurrentProcess().Kill();
                } else
                {
                    FallbackToSlave();
                }
            }
            else
            {
                gameState.Players.RemoveAll(player => player.Url == Url);
                gameState.Servers.RemoveAll(server => server.Url == Url);
            }
        }

        private void FallbackToSlave()
        {
            masterServer = gameState.Servers.OrderBy(server => server.Url).First().Url;
        }
    }

    internal class CommandQueue
    {
        private int delay = 0;

        private ConcurrentQueue<Command> queue;

        public CommandQueue()
        {
            queue = new ConcurrentQueue<Command>();
        }

        internal void SetDelay(int delay)
        {
            this.delay = delay;
        }

        internal void Enqueue(Command command)
        {
            command.InsertedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            queue.Enqueue(command);
        }

        internal Command Dequeue()
        {
            if (!queue.TryDequeue(out Command command))
            {
                return null;
            }

            long pendingDelay = -1;
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
                }
                catch (ThreadInterruptedException) { }
            }

            return command;
        }
    }

    internal class EndpointPool
    {
        private Dictionary<string, RemotingEndpoint> endpoints;

        public EndpointPool()
        {
            endpoints = new Dictionary<string, RemotingEndpoint>();
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