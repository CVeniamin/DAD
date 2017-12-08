using OGP.Middleware;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OGP.Server
{
    [Serializable]
    public class GameStateView
    {
        public List<Player> Players;
        public List<Ghost> Ghosts;
        public List<Coin> Coins;
        public List<Wall> Walls;
        public List<Server> Servers;
        public bool GameOver;
        public int RoundId;
    }

    public class GameState
    {
        public List<Player> Players { get; private set; }
        public List<Ghost> Ghosts { get; private set; }
        public List<Coin> Coins { get; private set; }
        public List<Wall> Walls { get; private set; }
        public List<Server> Servers { get; private set; }
        
        public bool GameOver { get; set; }
        public int RoundId { get; set; }

        public GameState(List<string> existsingServersList)
        {
            Players = new List<Player>();

            InitGhosts();
            InitCoins();
            InitWalls();

            LoadServers(existsingServersList);
        }

        private void InitGhosts()
        {
            Ghosts = new List<Ghost>();

            Ghost pink = new Ghost
            {
                X = GameConstants.PINK_GHOST.STARTING_X,
                Y = GameConstants.PINK_GHOST.STARTING_Y,
                DX = GameConstants.PINK_GHOST.VELOCITY_X,
                DY = GameConstants.PINK_GHOST.VELOCITY_Y,
                Color = GhostColor.Pink
            };
            Ghosts.Add(pink);

            Ghost yellow = new Ghost
            {
                X = GameConstants.YELLOW_GHOST.STARTING_X,
                Y = GameConstants.YELLOW_GHOST.STARTING_Y,
                DX = GameConstants.YELLOW_GHOST.VELOCITY_X,
                DY = GameConstants.YELLOW_GHOST.VELOCITY_Y,
                Color = GhostColor.Yellow
            };
            Ghosts.Add(yellow);

            Ghost red = new Ghost
            {
                X = GameConstants.RED_GHOST.STARTING_X,
                Y = GameConstants.RED_GHOST.STARTING_Y,
                DX = GameConstants.RED_GHOST.VELOCITY_X,
                DY = GameConstants.RED_GHOST.VELOCITY_Y,
                Color = GhostColor.Red
            };
            Ghosts.Add(red);
        }

        private void InitCoins()
        {
            Coins = new List<Coin>();

            short totalCoins = 41;

            short columnX = 1;
            short x = 15;
            short y = 45;

            for (var i = 1; i <= totalCoins; i++)
            {
                if (columnX == 7)
                {
                    x = 15;
                    y += 45;
                    columnX = 1;
                }

                Coin c = new Coin
                {
                    X = x,
                    Y = y,
                    Visible = true
                };

                x += 60;
                columnX++;

                Coins.Add(c);
            }
        }
        
        private void InitWalls()
        {
            Walls = new List<Wall>();

            Wall w1 = new Wall
            {
                X = 110,
                Y = 50,
                Width = 15,
                Height = 80
            };

            Wall w2 = new Wall
            {
                X = 280,
                Y = 50,
                Width = 20,
                Height = 60
            };

            Wall w3 = new Wall
            {
                X = 110,
                Y = 245,
                Width = 15,
                Height = 80
            };

            Wall w4 = new Wall
            {
                X = 300,
                Y = 245,
                Width = 20,
                Height = 60,
            };

            Walls.Add(w1);
            Walls.Add(w2);
            Walls.Add(w3);
            Walls.Add(w4);
        }

        internal Player GetPlayerByUrl(string Url)
        {
            return Players.Find(player => player.Url == Url);
        }

        internal GameStateView GetGameStateView()
        {
            Console.WriteLine("Returning new gameStateView");
            return new GameStateView
            {
                Players = Players,
                Ghosts = Ghosts,
                Coins = Coins,
                Walls = Walls,
                Servers = Servers,
                RoundId = RoundId,
                GameOver = GameOver
            };
        }

        private void LoadServers(List<string> existsingServersList)
        {
            Servers = new List<Server>();

            foreach (string Url in existsingServersList)
            {
                Servers.Add(new Server
                {
                    Url = Url
                });
            }
        }

        internal void AddServerIfNotExists(string source)
        {
            if (!Servers.Exists(server => server.Url == source))
            {
                Servers.Add(new Server
                {
                    Url = source
                });
            }
        }

        internal void AddPlayerIfNotExists(string Url, string Pid)
        {
            if (GetPlayerByUrl(Url) == null)
            {
                Players.Add(new Player {
                    X = 8,
                    Y = (Players.Count + 1) * 40,
                    PlayerId = Pid,
                    Url = Url,
                    Score = 0,
                    Alive = true,
                    Direction = Direction.RIGHT
                });
            }
        }

        public void Patch(GameStateView newGameStateView)
        {
            Players = newGameStateView.Players;
            Ghosts = newGameStateView.Ghosts;
            Coins = newGameStateView.Coins;
            Walls = newGameStateView.Walls;
            Servers = newGameStateView.Servers;
            RoundId = newGameStateView.RoundId;
            GameOver = newGameStateView.GameOver;
        }
    }
}