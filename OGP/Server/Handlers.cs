using System;

namespace OGP.Server
{
    internal class ActionHandler
    {
        public ActionHandler()
        {
            // TODO: set up timer
        }

        internal void Process(object args)
        {
            // Up/down/left/right
        }
    }

    internal class ChatHandler
    {
        public ChatHandler()
        {
            
        }

        internal void Process(object args)
        {
            // Chat message received
        }
    }

    internal class StateHandler
    {
        public StateHandler()
        {
            
        }

        internal void Process(object args)
        {
            // Received state from other server (master?) check and apply
        }
    }
}