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
        public ConcurrentBag<Coin> Coins;
        public List<Wall> Walls;
        public List<Server> Servers;
        public bool GameOver;
        public int RoundId;
    }

    public class GameState
    {
        private GameStateView cachedGameStateView = null;

        public List<Player> Players { get; private set; }
        public List<Ghost> Ghosts { get; private set; }
        public ConcurrentBag<Coin> Coins { get; private set; }
        public List<Wall> Walls { get; private set; }
        public List<Server> Servers { get; private set; }

        private bool gameStarted;
        private bool gameOver;
        public bool GameStarted { get => gameStarted; set => gameStarted = value; }
        public bool GameOver { get => gameOver; set => gameOver = value; }
        public int RoundId { get;  set; }

        public GameState(List<string> existsingServersList)
        {
            this.GameStarted = false;

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
                X = 200,
                Y = 50,
                Color = GhostColor.Pink,
                Width = 35,
                Height = 35
            };

            Ghost yellow = new Ghost
            {
                X = 200,
                Y = 235,
                Color = GhostColor.Yellow,
                Width = 35,
                Height = 35
            };

            Ghost red = new Ghost
            {
                X = 240,
                Y = 90,
                Color = GhostColor.Red,
                Width = 35,
                Height = 35
            };

            Ghosts.Add(pink);
            Ghosts.Add(yellow);
            Ghosts.Add(red);
        }

        private void InitCoins()
        {
            Coins = new ConcurrentBag<Coin>();

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
                    Width = 15,
                    Height = 15
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
            if (cachedGameStateView == null)
            {
                GenerateGameStateView();
                return cachedGameStateView; // Return newly generated state
            }

            return cachedGameStateView; // Return null or cached game state
        }

        private void GenerateGameStateView()
        {
            cachedGameStateView = new GameStateView
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
                Players.Add(new Player { X = 8, Y = (Players.Count + 1) * 40, PlayerId = Pid, Url = Url, Score = 0, Alive = true, Width = 35, Height = 35});
            }
        }

        internal void Patch(GameStateView newGameStateView)
        {
            // TODO: newGameState
        }
    }
}