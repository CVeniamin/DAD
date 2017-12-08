using System;
using System.Collections.Generic;
using System.Threading;

namespace OGP.Server
{
    public interface IHandler
    {
        void Process(string source, object args);
        void SetOutManager(OutManager outManager);
    }

    public class ActionHandler : IHandler
    {
        private HashSet<string> stateDispatchDestinations;
        private OutManager outManager;
        private GameState gameState;
        private int numPlayers;

        private bool gameOn = false;
        private Object dispatchDestinationLock = new Object();

        public ActionHandler(GameState gameState, int numPlayers)
        {
            stateDispatchDestinations = new HashSet<string>();
            this.gameState = gameState;
            this.numPlayers = numPlayers;
            this.gameState.RoundId = -1;
        }

        public void Process(string source, object action)
        {
            Console.WriteLine("Received Command from {0}", source);

            if (action is GameMovement movement)
            {
                // Resolve player URL to Player object
                Player player = gameState.GetPlayerByUrl(source);

                if (player == null)
                {
                    Console.WriteLine("Player with {0} URL not found", source);
                    return;
                }

                if (gameOn)
                {
                    player.Direction = movement.Direction;
                }
                else
                {
                    Console.WriteLine("Movement before game start ignored");
                }
            }
            else if (action is ClientSync sync)
            {
                // State request from an idle client
                if (!gameOn && gameState.Players.Count < numPlayers)
                {
                    gameState.AddPlayerIfNotExists(source, sync.Pid);
                }
            }
            else
            {
                // State request from a slave
                gameState.AddServerIfNotExists(source);
            }

            lock (dispatchDestinationLock)
            {
                stateDispatchDestinations.Add(source);
            }
        }

        internal void FinilizeTick(long tickId)
        {
            // Finish processing the tick, write state to file, etc.
            // tell gamestate current roundId , everyone receive gamestate on his roundId
            
            /*if(!gameStarted){
             *  roundId = tickId; 
             *  gameState.RoundId = roundId;
             * 
                CheckIfGameReady();
             }else{
                DrawThings();
             }*/

            MovePlayers(); // This will stop movement if player hits the wall
            // MoveGhosts(); // This will kill players
            // CollectCoins(); // This will update player scores for alive players

            // WriteState(); // This should write the game state to file or standard output or something

            DispatchState();
        }

        private void MovePlayers()
        {
            foreach (Player player in gameState.Players)
            {
                switch (player.Direction)
                {
                    case Direction.UP:
                        player.Y--;
                        break;
                    case Direction.DOWN:
                        player.Y++;
                        break;
                    case Direction.LEFT:
                        player.X--;
                        break;
                    case Direction.RIGHT:
                        player.X++;
                        break;
                }
            }
        }

        private void DispatchState()
        {
            if (this.stateDispatchDestinations.Count == 0)
            {
                return;
            }

            // Console.WriteLine("Notifying {0} recipients of current state", this.stateDispatchDestinations.Count);
            lock (dispatchDestinationLock)
            {
                foreach (string Url in this.stateDispatchDestinations)
                {
                    outManager.SendCommand(new Command
                    {
                        Type = CommandType.State,
                        Args = gameState.GetGameStateView()
                    }, Url);
                }
                this.stateDispatchDestinations.Clear();
            }
            
        }
        
        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }

    public class ChatHandler : IHandler
    {
        private OutManager outManager;
        private Action<ChatMessage> onChatMessage;

        public ChatHandler(Action<ChatMessage> onChatMessage)
        {
            this.onChatMessage = onChatMessage;
        }

        public void Process(string source, object args)
        {
            // TODO: could do due diligence
            onChatMessage((ChatMessage)args);
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }

    public class StateHandler : IHandler
    {
        private OutManager outManager;
        private Action<GameStateView> onNewGameStateView;

        public StateHandler(Action<GameStateView> onNewGameStateView)
        {
            this.onNewGameStateView = onNewGameStateView;
        }

        public void Process(string source, object args)
        {
            try
            {
                onNewGameStateView((GameStateView)args);
            }
            catch (ThreadInterruptedException) { }
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }
}