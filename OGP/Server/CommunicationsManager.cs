using System;
using System.Collections.Generic;
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

    internal class InManager
    {
        private bool initError = false;
        private bool frozen = false;

        private ActionHandler actionHandler;
        private ChatHandler chatHandler;
        private StateHandler stateHandler;

        private CommandQueue commandQueue;
        private Thread backgroundThread;

        public InManager(string Url, ActionHandler actionHandler, ChatHandler chatHandler, StateHandler stateHandler)
        {
            Uri uri = new Uri(Url);

            try
            {
                TcpChannel channel = new TcpChannel(uri.Port);
                ChannelServices.RegisterChannel(channel, true);
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not bind to port. Either already occupied or blocked by firewall. Exiting.", "CRITICAL");
                initError = true;
                return;
            }

            RemotingEndpoint endpoint = new RemotingEndpoint(this);
            RemotingServices.Marshal(endpoint, uri.AbsolutePath.Substring(1));

            this.actionHandler = actionHandler;
            this.chatHandler = chatHandler;
            this.stateHandler = stateHandler;

            this.commandQueue = new CommandQueue();

            backgroundThread = new Thread(() =>
            {
                while (true)
                {
                    if (!frozen)
                    {
                        Command command = commandQueue.Dequeue();
                        if (command != null)
                        {
                            PassCommandToHandler(command);
                        } // else - queue empty
                    }

                    Thread.Sleep(Timeout.Infinite);
                }
            });

            backgroundThread.Start();

            Console.WriteLine("Server Registered at " + Url);
        }

        public bool GotError()
        {
            return initError;
        }

        internal void Enqueue(Command command)
        {
            commandQueue.Enqueue(command);
        }

        public void PassCommandToHandler(Command command)
        {
            switch (command.Type)
            {
                case Type.Action:
                    actionHandler.Process(command.Args);
                    break;

                case Type.Chat:
                    chatHandler.Process(command.Args);
                    break;

                case Type.State:
                    stateHandler.Process(command.Args);
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

    internal enum Type
    { Action, Chat, State };

    [Serializable]
    internal class Command
    {
        public Type Type { get; set; }
        public object Args { get; set; }
        internal long InsertedTime { get; set; }
    }

    internal class OutManager
    {
        private Dictionary<string, CommandQueue> outQueues;

        private Thread backgroundThread;
        private HashSet<CommandQueue> activeQueues;

        public OutManager()
        {
            this.outQueues = new Dictionary<string, CommandQueue>();
            this.activeQueues = new HashSet<CommandQueue>();

            backgroundThread = new Thread(() =>
            {
                while (true)
                {
                    foreach (CommandQueue commandQueue in activeQueues)
                    {
                        Command command = commandQueue.Dequeue();
                    }
                    activeQueues.Clear();

                    Thread.Sleep(Timeout.Infinite);
                }
            });

            backgroundThread.Start();
        }

        public bool SendCommand(Command command, string Url)
        {
            if (!outQueues.TryGetValue(Url, out CommandQueue commandQueue))
            {
                commandQueue = new CommandQueue();
                commandQueue.Enqueue(command);
                outQueues.Add(Url, commandQueue);
            }

            commandQueue.Enqueue(command);

            activeQueues.Add(commandQueue);

            return true;
        }

        public void SetDelay(string dstUrl, int time)
        {
            if (outQueues.TryGetValue(dstUrl, out CommandQueue commandQueue))
            {
                commandQueue.SetDelay(time);
            }
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

            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long pendingDelay = currentTime - command.InsertedTime - delay;

            if (pendingDelay < 0)
            {
                Thread.Sleep(-(int)pendingDelay);
            }

            return command;
        }
    }
}