using OGP.Middleware;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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

        private long gameStartTickId = -1;
        private Object dispatchDestinationLock = new Object();

        public ActionHandler(GameState gameState, int numPlayers)
        {
            stateDispatchDestinations = new HashSet<string>();

            this.gameState = gameState;
            this.numPlayers = numPlayers;
            this.gameState.RoundId = -1;
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
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

                if (gameStartTickId >= 0 && !gameState.GameOver)
                {
                    player.Direction = movement.Direction;
                }
                else
                {
                    Console.WriteLine("Out of round movement ignored");
                }
            }
            else if (action is ClientSync sync)
            {
                // State request from an idle client
                if (gameStartTickId == -1 && gameState.Players.Count < numPlayers)
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
            if (gameStartTickId == -1)
            {
                if (gameState.Players.Count == 0)
                {
                    if (tickId % 4 == 0)
                    {
                        Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
                        Console.Write("Waiting for players");
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }

                StartGameIfReady(tickId);
            }
            else if (gameState.GameOver)
            {
                Console.WriteLine("Game over");
                // Stop state updates? Force clients to quit? Send special message? Or clients already know gameState.GameOver == true
                // TODO
            } else
            {
                gameState.RoundId = (int)(tickId - gameStartTickId);

                Console.WriteLine("Processing round {0}", gameState.RoundId);

                MovePlayers(); // This will stop movement if player hits the wall
                MoveGhosts(); // This will kill players
                CollectCoins(); // This will update player scores for alive players

                CheckIfGameOver();
                
                WriteState(); // This should write the game state to file or standard output or something
            }
            
            DispatchState();
        }

        private void StartGameIfReady(long tickId)
        {
            if (gameState.Players.Count >= numPlayers)
            {
                gameStartTickId = tickId;
                gameState.RoundId = 0;

                Console.WriteLine("Game started");
            }
        }

        private void CheckIfGameOver()
        {
            int alivePlayers = 0;
            foreach (Player player in gameState.Players)
            {
                if (player.Alive) {
                    alivePlayers++;
                }
            }

            if (alivePlayers == 0)
            {
                gameState.GameOver = true;
                return;
            }

            int availableCoins = 0;
            foreach (Coin coin in gameState.Coins)
            {
                if (coin.Visible)
                {
                    availableCoins++;
                }
            }

            if (availableCoins == 0)
            {
                gameState.GameOver = true;
                return;
            }
        }

        private void MovePlayers()
        {
            foreach (Player player in gameState.Players)
            {
                if (!player.Alive)
                {
                    continue;
                }

                if (DetectPlayerWallCollision(player, gameState.Walls))
                {
                    player.Direction = Direction.NONE;
                    continue;
                }

                switch (player.Direction)
                {
                    case Direction.UP:
                        player.Y -= GameConstants.PLAYER_SPEED;
                        break;
                    case Direction.DOWN:
                        player.Y += GameConstants.PLAYER_SPEED;
                        break;
                    case Direction.LEFT:
                        player.X -= GameConstants.PLAYER_SPEED;
                        break;
                    case Direction.RIGHT:
                        player.X += GameConstants.PLAYER_SPEED;
                        break;
                }
            }
        }

        private void MoveGhosts()
        {
            foreach (Ghost ghost in gameState.Ghosts)
            {
                // TODO: check if ghost will hit and obstacle if it completes the move
                // New coordinate:
                //ghost.X + ghost.DX
                //ghost.Y + ghost.DY

                /*if (willHitVerticalObstacle())
                {
                    ghost.DX = -ghost.DX;
                }

                if (willHitHorizontalObstacle())
                {
                    ghost.DY = -ghost.DY;
                }*/

                ghost.X += ghost.DX;
                ghost.Y += ghost.DY;

                foreach (Player player in gameState.Players)
                {
                    if (DetectPlayerGhostCollision(player, ghost))
                    {
                        player.Alive = false;
                    }
                }
            }
        }

        private void DispatchState()
        {
            if (stateDispatchDestinations.Count == 0)
            {
                return;
            }
            
            lock (dispatchDestinationLock)
            {
                GameStateView gameStateView = gameState.GetGameStateView();

# if DEBUG
                Console.WriteLine("Notifying {0} recipients of current state (we have {0} players)", stateDispatchDestinations.Count, gameStateView.Players.Count);
# endif

                foreach (string Url in stateDispatchDestinations)
                {
                    outManager.SendCommand(new Command
                    {
                        Type = CommandType.State,
                        Args = gameStateView
                    }, Url);
                }
                stateDispatchDestinations.Clear();

                gameStateView = null;
            }
        }

        private void CollectCoins()
        {
            foreach (Coin coin in gameState.Coins)
            {
                if (!coin.Visible)
                {
                    continue;
                }

                foreach (Player player in gameState.Players)
                {
                    if (player.Alive && DetectPlayerCoinCollision(player, coin))
                    {
                        player.Score++;
                        coin.Visible = false;
                        break;
                    }
                }
            }
        }

        private bool DetectPlayerCoinCollision(Player player, Coin coin)
        {
            return (player.X < coin.X + ObjectDimensions.COIN_WIDTH
                        && player.X + ObjectDimensions.PLAYER_WIDTH > coin.X
                        && player.Y < coin.Y + ObjectDimensions.COIN_HEIGHT
                        && player.Y + ObjectDimensions.PLAYER_HEIGHT > coin.Y);
        }
        
        private bool DetectPlayerGhostCollision(Player player, Ghost ghost)
        {
            return (player.X < ghost.X + ObjectDimensions.GHOST_WIDTH
                        && player.X + ObjectDimensions.PLAYER_WIDTH > ghost.X
                        && player.Y < ghost.Y + ObjectDimensions.GHOST_HEIGHT
                        && player.Y + ObjectDimensions.PLAYER_HEIGHT > ghost.Y);
        }

        private bool DetectPlayerWallCollision(Player player, List<Wall> walls)
        {
            // TODO
            return false;
        }

        private void WriteState()
        {
            // TODO
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
        private Object invokeLock = new Object();
        
        public StateHandler(Action<GameStateView> onNewGameStateView)
        {
            this.onNewGameStateView = onNewGameStateView;
        }

        public void Process(string source, object args)
        {
            try
            {
                onNewGameStateView?.Invoke((GameStateView)args);
            }
            catch (Exception ex) {
# if DEBUG
                Console.WriteLine("Process got exception: " + ex.Message);
# endif
            }
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }
}