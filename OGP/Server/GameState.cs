using OGP.Middleware;
using System;
using System.Collections.Generic;
using System.Text;

namespace OGP.Server
{
    [Serializable]
    public class GameStateView
    {
        public List<Player> Players;
        public List<Ghost> Ghosts;
        public List<Coin> Coins;
        public List<Wall> Walls;
        public List<GameServer> Servers;
        public bool GameOver;
        public int RoundId;
        public Dictionary<int, string> PreviousGames;
    }

    public class GameState
    {
        public List<Player> Players { get; private set; }
        public List<Ghost> Ghosts { get; private set; }
        public List<Coin> Coins { get; private set; }
        public List<Wall> Walls { get; private set; }
        public List<GameServer> Servers { get; private set; }

        public bool GameOver { get; set; }
        public int RoundId { get; set; }

        public Dictionary<int, string> PreviousGames;

        public GameState(List<string> existsingServersList)
        {
            Players = new List<Player>();
            PreviousGames = new Dictionary<int, string>();
            RoundId = -1;

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
                Width = 10,
                Height = 80
            };

            Wall w2 = new Wall
            {
                X = 280,
                Y = 50,
                Width = 15,
                Height = 45
            };

            Wall w3 = new Wall
            {
                X = 110,
                Y = 245,
                Width = 15,
                Height = 90
            };

            Wall w4 = new Wall
            {
                X = 300,
                Y = 245,
                Width = 15,
                Height = 40,
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
            Servers = new List<GameServer>();

            foreach (string Url in existsingServersList)
            {
                Servers.Add(new GameServer
                {
                    Url = Url
                });
            }
        }

        internal void AddServerIfNotExists(string source, long tickId)
        {
            GameServer existingServer = Servers.Find(server => server.Url == source);
            if (existingServer == null)
            {
                existingServer = new GameServer
                {
                    Url = source
                };
                Servers.Add(existingServer);
            }

            existingServer.LastAlive = tickId;
        }

        internal void AddPlayerIfNotExists(string Url, string Pid)
        {
            if (GetPlayerByUrl(Url) == null)
            {
                Players.Add(new Player
                {
                    X = 8,
                    Y = (Players.Count + 1) * 40,
                    PlayerId = Pid,
                    Url = Url,
                    Score = 0,
                    Alive = true,
                    Direction = Direction.NONE
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

            if (RoundId >= 0 && !PreviousGames.ContainsKey(RoundId))
            {
                PreviousGames.Add(RoundId, WriteState());
            }
        }

        public String WriteState()
        {
            StringBuilder outputString = new StringBuilder();

            foreach (Ghost ghost in this.Ghosts)
            {
                outputString.AppendLine(String.Format("M, {0}, {1}", ghost.X, ghost.Y));
            }

            foreach (Wall wall in this.Walls)
            {
                outputString.AppendLine(String.Format("W, {0}, {1}", wall.X, wall.Y));
            }

            foreach (Player player in this.Players)
            {
                outputString.AppendLine(String.Format("{0}, {1}, {2}, {3}", player.PlayerId, player.Alive ? "P" : "L", player.X, player.Y));
            }

            foreach (Coin coin in this.Coins)
            {
                if (coin.Visible)
                {
                    outputString.AppendLine(String.Format("C, {0}, {1}", coin.X, coin.Y));
                }
            }

            return outputString.ToString();
        }
    }
}