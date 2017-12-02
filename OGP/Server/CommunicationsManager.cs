using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OGP.Server
{
    class RemotingEndpoint : MarshalByRefObject
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

    class InManager
    {
        private bool initError = false;

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
            
            Console.WriteLine("Server Registered at " + Url);
        }

        public bool GotError()
        {
            return initError;
        }

        internal void Freeze()
        {
            throw new NotImplementedException();
        }

        internal void Unfreeze()
        {
            throw new NotImplementedException();
        }
    }

    enum Type { Action, Chat, State };

    [Serializable]
    class Command
    {
        public Type Type { get; set; }
        public object Args { get; set; }
        internal long InsertedTime { get; set; }
    }

    class OutManager
    {
        private object outQueue; // store all messages before sending

        public OutManager()
        {

            // TODO: launch thread that will send messages
        }

        public bool SendCommand(Command command, string Url)
        {
            // TODO: add command to queue
            return true;
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
            Command command = queue.Dequeue();

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
