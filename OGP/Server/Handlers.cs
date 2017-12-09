using OGP.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
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

        private long currentTickId = 0;
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
                gameState.AddServerIfNotExists(source, currentTickId);
            }
            try
            {
                lock (dispatchDestinationLock)
                {
                    stateDispatchDestinations.Add(source);
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        internal void FinalizeTick(long tickId)
        {
            if (gameStartTickId == -1 && gameState.RoundId == -1)
            {
                StartGameIfReady(tickId);
            }
            else if (!gameState.GameOver)
            {
                if (gameStartTickId == -1)
                {
                    // We became master, but know of it only now
                    Console.WriteLine("New master was born");
                    gameStartTickId = tickId - gameState.RoundId - 1;
                    gameState.RoundId++;
                }
                else
                {
                    gameState.RoundId = (int)(tickId - gameStartTickId);
                    gameState.PreviousGames.Add(gameState.RoundId, gameState.WriteState());
                }
                
                MovePlayers(); // This will stop movement if player hits the wall
                MoveGhosts(); // This will kill players
                CollectCoins(); // This will update player scores for alive players

                CheckIfGameOver();
            }
            
            DispatchState();

            currentTickId = tickId;
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
                if (player.Alive)
                {
                    alivePlayers++;
                }
            }

            if (alivePlayers == 0)
            {
                Console.WriteLine("Game Over: everybody is dead");
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
                Console.WriteLine("Game Over: all coins collected");
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

                int DY = 0;
                int DX = 0;

                switch (player.Direction)
                {
                    case Direction.UP:
                        DY = -GameConstants.PLAYER_SPEED;
                        break;
                    case Direction.DOWN:
                        DY = GameConstants.PLAYER_SPEED;
                        break;

                    case Direction.LEFT:
                        DX = -GameConstants.PLAYER_SPEED;
                        break;

                    case Direction.RIGHT:
                        DX = GameConstants.PLAYER_SPEED;
                        break;
                }

                if (PlayerHitsObstacle(player, DX, DY))
                {
                    continue;
                }

                player.Y += DY;
                player.X += DX;
            }
        }

        private void MoveGhosts()
        {
            foreach (Ghost ghost in gameState.Ghosts)
            {
                if (ghost.DX != 0 && (WillHitVerticalBoardEdge(ghost.X, ObjectDimensions.GHOST_WIDTH) || WillHitWall(new BoundingBox
                {
                    X1 = ghost.X + ghost.DX,
                    X2 = ghost.X + ghost.DX + ObjectDimensions.GHOST_WIDTH,
                    Y1 = ghost.Y,
                    Y2 = ghost.Y + ObjectDimensions.GHOST_HEIGHT
                })))
                {
                    ghost.DX = -ghost.DX;
                }
                
                if (ghost.DY != 0 && (WillHitHorizontalBoardEdge(ghost.Y, ObjectDimensions.GHOST_HEIGHT) || WillHitWall(new BoundingBox
                {
                    X1 = ghost.X,
                    X2 = ghost.X + ObjectDimensions.GHOST_WIDTH,
                    Y1 = ghost.Y + ghost.DY,
                    Y2 = ghost.Y + ghost.DY + ObjectDimensions.GHOST_HEIGHT
                })))
                {
                    ghost.DY = -ghost.DY;
                }
                
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

        private bool PlayerHitsObstacle(Player player, int dx, int dy)
        {
            int newX = player.X + dx;
            int newY = player.Y + dy;

            // Check board edges
            if (WillHitVerticalBoardEdge(newX, ObjectDimensions.PLAYER_WIDTH) || WillHitHorizontalBoardEdge(newY, ObjectDimensions.PLAYER_WIDTH))
            {
                return true;
            }

            // Check walls
            foreach (Wall wall in gameState.Walls)
            {
                if (DetectCollision(newX, newY, ObjectDimensions.PLAYER_WIDTH, ObjectDimensions.PLAYER_HEIGHT, wall.X, wall.Y, wall.Width, wall.Height))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private bool WillHitWall(BoundingBox boundingBox)
        {
            foreach (Wall wall in gameState.Walls)
            {
                if (DetectCollision(boundingBox, wall.X, wall.Y, wall.Width, wall.Height))
                {
                    return true;
                }
            }

            return false;
        }

        private bool WillHitVerticalBoardEdge(int x, int width)
        {
            return x < 0 || x + width > ObjectDimensions.BOARD_WIDTH;
        }

        private bool WillHitHorizontalBoardEdge(int y, int height)
        {
            return y < 0 || y + height > ObjectDimensions.BOARD_HEIGHT;
        }
        
        private bool DetectPlayerCoinCollision(Player player, Coin coin)
        {
            return (DetectCollision(player.X, player.Y, ObjectDimensions.PLAYER_WIDTH, ObjectDimensions.PLAYER_HEIGHT, 
                                    coin.X, coin.Y, ObjectDimensions.COIN_WIDTH, ObjectDimensions.COIN_HEIGHT));
        }

        private bool DetectPlayerGhostCollision(Player player, Ghost ghost)
        {
            return DetectCollision(player.X, player.Y, ObjectDimensions.PLAYER_WIDTH, ObjectDimensions.PLAYER_HEIGHT, 
                                   ghost.X, ghost.Y, ObjectDimensions.GHOST_WIDTH, ObjectDimensions.GHOST_HEIGHT);
        }

        private bool DetectCollision(int x1, int y1, int width1, int height1, int x2, int y2, int width2, int height2)
        {
            return x1 < x2 + width2 && x1 + width1 > x2 && y1 < y2 + height2 && y1 + height1 > y2;
        }

        private bool DetectCollision(BoundingBox a, int x, int y, int width, int height)
        {
            return a.X1 < x + width && a.X2 > x && a.Y1 < y + height && a.Y2 > y;
        }

        private void DispatchState()
        {
            lock (dispatchDestinationLock)
            {
                if (stateDispatchDestinations.Count == 0)
                {
                    return;
                }

                GameStateView gameStateView = gameState.GetGameStateView();
                foreach (string Url in stateDispatchDestinations)
                {
                    outManager.SendCommand(new Command
                    {
                        Type = CommandType.State,
                        Args = gameStateView
                    }, Url);
                }

                stateDispatchDestinations.Clear();
            }
        }
    }

    internal class BoundingBox
    {
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
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
            catch (Exception ex)
            {
# if DEBUG
                Console.WriteLine("Process got exception: " + ex.Message + ex.StackTrace);
# endif
            }
        }

        public void SetOutManager(OutManager outManager)
        {
            this.outManager = outManager;
        }
    }
}