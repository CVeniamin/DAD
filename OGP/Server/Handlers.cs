using System;
using System.Collections.Generic;

namespace OGP.Server
{
    public interface IHandler
    {
        void Process(string source, object args);
        void SetOutManager(OutManager outManager);
    }

    public class ActionHandler : IHandler
    {
        private HashSet<string> stateRequestors;
        private OutManager outManager;
        private GameState gameState;

        public ActionHandler(GameState gameState)
        {
            stateRequestors = new HashSet<string>();
            this.gameState = gameState;

            // TODO: set up timer
        }

        public void Process(string source, object args)
        {
            Console.WriteLine("got command on server");
            Console.WriteLine(args);

            // Up/down/left/right

            stateRequestors.Add(source);
        }

        internal void NotifyOfState()
        {
            if (this.stateRequestors.Count == 0)
            {
                return;
            }

            Console.WriteLine("notifying of state to " + this.stateRequestors.Count);
            foreach (string Url in this.stateRequestors)
            {
                outManager.SendCommand(new Command
                {
                    Type = Type.State,
                    Args = this.gameState.GetGameState()
                }, Url);
            }
            this.stateRequestors.Clear();
        }
        
        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }

    public class ChatHandler : IHandler
    {

        private OutManager outManager;

        public ChatHandler()
        {
            
        }

        public void Process(string source, object args)
        {
            // Chat message received
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }

    public class StateHandler : IHandler
    {
        private OutManager outManager;
        public delegate void StateDelegate(GameStateView view);

        public StateHandler(StateDelegate stateDelegate)
        {
            
        }

        public void Process(string source, object args)
        {
            GameStateView gameStateView = (GameStateView)args;
            Console.WriteLine("Ghosts count: " + gameStateView.Ghosts.Count);
            
            // Received state from other server (master?) check and apply
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }
}