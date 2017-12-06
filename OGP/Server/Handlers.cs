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
        public enum Move
        { UP, DOWN, LEFT, RIGHT };

        private HashSet<string> stateRequestors;
        private OutManager outManager;
        private Game game;
        private GameState gameState;

        public ActionHandler(Game game)
        {
            stateRequestors = new HashSet<string>();
            this.game = game;
            this.gameState = game.GameState;

            // TODO: set up timer
        }

        public void Process(string source, object move)
        {
            //Player player = (Player)o;
            //Console.WriteLine(player.PlayerId);
            //switch (player.Move)
            //{
            //    case Move.UP:
            //        game.MoveUp(player.PlayerId);
            //        break;
            //    case Move.DOWN:
            //        game.MoveDown(player.PlayerId);
            //        break;
            //    case Move.LEFT:
            //        game.MoveLeft(player.PlayerId);
            //        break;
            //    case Move.RIGHT:
            //        game.MoveRight(player.PlayerId);
            //        break;
            //}
            Console.WriteLine(source);
            Console.WriteLine(move);
            switch (move)
            {
                case "UP":
                    gameState.MoveCoin();
                    int x = gameState.MovePlayerUp(source);
                    Console.WriteLine("x" + x);
                    break;
                //case "DOWN":
                //    game.MoveDown(source);
                //    break;
                //case "LEFT":
                //    game.MoveLeft(source);
                //    break;
                //case "RIGHT":
                //    game.MoveRight(source);
                //    break;
            }
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
                    Args = this.game.GameState
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
        //public delegate void StateDelegate(GameStateView view);

        public StateHandler()
        {
            
        }

        public void Process(string source, object args)
        {
            GameState gameState = (GameState) args;
            Console.WriteLine("Ghosts count: " + gameState.Ghosts.Count);
            
            // Received state from other server (master?) check and apply
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }
}