﻿using OGP.Middleware;
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
            //Console.WriteLine("Received Command from {0}", source);

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
                    Console.WriteLine("DIRECTION {0} : {1} :" , player.Direction, player.PlayerId);
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
            }
            else
            {
                gameState.RoundId = (int)(tickId - gameStartTickId);

                //Console.WriteLine("Processing round {0}", gameState.RoundId);

                //Add this game to previous games, used later for LocalState command
                gameState.PreviousGames.Add(gameState.RoundId, gameState.WriteState());

                MovePlayers(); // This will stop movement if player hits the wall
                MoveGhosts(); // This will kill players
                CollectCoins(); // This will update player scores for alive players

                CheckIfGameOver();
                
                //gameState.WriteState(); // This should write the game state to file or standard output or something
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
                if (player.Alive)
                {
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
                if(PlayerHitsObstacle(player, DX, DY))
                {
                    Console.WriteLine("player.x {0} player.Y {1} DX {2} DY {3} ", player.X, player.Y, DX, DY);
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
                if (GhostHitsVerticalObstacle(ghost))
                {
                    ghost.DY = -ghost.DY;
                }

                if (GhostHitsHorizontalObstacle(ghost))
                {
                    ghost.DX = -ghost.DX;
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
                //Console.WriteLine("Notifying {0} recipients of current state (we have {0} players)", stateDispatchDestinations.Count, gameStateView.Players.Count);
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

        private Direction SwitchDirection(Player player)
        {
            Direction direction = player.Direction;
            switch (direction)
            {
                case Direction.UP:
                    return Direction.DOWN;
                case Direction.LEFT:
                    return Direction.RIGHT;
                case Direction.RIGHT:
                    return Direction.LEFT;
                case Direction.DOWN:
                    return Direction.UP;
            }
            return direction;
        }
        private bool PlayerHitsVerticalObstacle(Player player)
        {
            return (WillHitVerticalBoard(player.Y, ObjectDimensions.PLAYER_HEIGHT));
        }
        private bool PlayerHitsHorizontalObstacle(Player player)
        {
            return (WillHitHorizontalBoard(player.X , ObjectDimensions.PLAYER_WIDTH));
        }
        private bool PlayerHitsObstacle(Player player, int dx, int dy)
        {
            if (WillHitHorizontalBoard(player.X + dx, ObjectDimensions.PLAYER_WIDTH) 
                || WillHitVerticalBoard(player.Y + dy, ObjectDimensions.BOARD_HEIGHT) 
                || DetectPlayerWallCollision(player, dx , dy))
            {
                return true;
            }
            return false;
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
        private bool DetectPlayerWallCollision(Player player, int dx, int dy)
        {
            foreach (Wall wall in gameState.Walls)
            {
                if (DetectCollision(player.X + dx, player.Y + dy , ObjectDimensions.PLAYER_WIDTH, ObjectDimensions.PLAYER_HEIGHT,
                                    wall.X, wall.Y, wall.Width, wall.Height))
                {
                    return true;
                }
            }
            return false;
        }

        private bool GhostHitsVerticalObstacle(Ghost ghost)
        {
            return (WillHitVerticalBoard(ghost.Y, ObjectDimensions.GHOST_HEIGHT) || DetectGhostWallCollision(ghost));
            
        }
        private bool GhostHitsHorizontalObstacle(Ghost ghost)
        {
            return (WillHitHorizontalBoard(ghost.X, ObjectDimensions.GHOST_WIDTH) || DetectGhostWallCollision(ghost));
        }
        private bool DetectGhostWallCollision(Ghost ghost)
        {
            foreach (Wall wall in gameState.Walls)
            {
                if (DetectCollision(ghost.X, ghost.Y, ObjectDimensions.GHOST_WIDTH, ObjectDimensions.GHOST_HEIGHT,
                                    wall.X, wall.Y, wall.Width, wall.Height))
                {
                    return true;
                }
            }
            return false;
        }

        private bool WillHitVerticalBoard(int y, int height)
        {
            return ((y - height <= 0 || y + height >= ObjectDimensions.BOARD_HEIGHT));
        }
        private bool WillHitHorizontalBoard(int x, int width)
        {
            return ((x - width <= 0 || x + width >= ObjectDimensions.BOARD_WIDTH));
        }
        private bool DetectCollision(int x1, int y1, int width1, int height1, int x2, int y2, int width2, int height2)
        {
            return (x1 < x2 + width2
                        && x1 + width1 > x2
                        && y1 < y2 + height2
                        && y1 +height1 > y2);
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
            Console.WriteLine("Got message on ChatHandler from : " + source + ((ChatMessage)args).Message.ToString());
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