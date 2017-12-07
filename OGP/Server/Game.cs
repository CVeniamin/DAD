using System;
using System.Collections.Generic;

namespace OGP.Server
{
    public interface IGame
    {
        bool GameOver { get; set; }

        void InitElements(int numOfCoins);

        void MoveLeft(string playerId);

        void MoveRight(string playerId);

        void MoveUp(string playerId);

        void MoveDown(string playerId);

        void RegisterPlayer(string endpoint, int x, int y);
    }

    public interface IGameService
    {
        bool AddGame(Game g);
    }

    [Serializable]
    public class GameClient
    {
        private String URL { get; set; }

        private Player player;
        public Player Player { get => player; set => player = value; }

        public GameClient(String url, Player player)
        {
            this.URL = url;
            this.player = player;
        }
    }

    public class Game : MarshalByRefObject, IGame
    {
        private int tickDuration;
        private int minimumPlayers = 1;
        private int gameID;
        private bool gameOver;
        private GameState gameState;
        private List<Player> players;

        public int TickDuration { get => tickDuration; set => tickDuration = value; }
        public int MinimumPlayers { get; set; }
        public int GameID { get => gameID; set => gameID = value; }
        public bool GameOver { get => gameOver; set => gameOver = value; }
        public GameState GameState { get => gameState; set => gameState = value; }
        public List<Player> Players { get => players; set => players = value; }

        public Game()
        {
        }

        public Game(GameState state, int tickDuration, int minimumPlayers, int gameID)
        {
            this.gameState = state;
            this.gameOver = false;
            this.tickDuration = tickDuration;
            this.minimumPlayers = minimumPlayers;
            this.gameID = gameID;
            this.players = new List<Player>();
        }

        public void InitElements(int numOfCoins)
        {
            if(gameState != null)
            {
                gameState.Walls = CreateWalls();
                gameState.Coins = CreateCoins(numOfCoins);
                gameState.Ghosts = CreateGhosts();
                
            }
        }

        public void InitPlayers(List<string> clientsEndpoints)
        {
            if (gameState != null)
            {
                Console.Write("Init Players at " + DateTimeOffset.Now.ToUnixTimeMilliseconds());
                gameState.Players = CreatePlayers(clientsEndpoints, 8); //x-axis = 8
            }
        }

        public void MoveUp(string playerId)
        {
            //Player p = gameState.GetPlayerByID(playerId);
            //gameState.MovePlayerUp(ref p);
        }

        public void MoveDown(string playerId)
        {
            //Player p = gameState.GetPlayerByID(playerId);
            //gameState.MovePlayerDown(ref p);
        }

        public void MoveLeft(string playerId)
        {
            Player p = gameState.GetPlayerByID(playerId);
            gameState.MovePlayerLeft(ref p);
        }

        public void MoveRight(string playerId)
        {
            Player p = gameState.GetPlayerByID(playerId);
            gameState.MovePlayerRight(ref p);
        }

        public void RegisterPlayer(string endpoint, int x, int y)
        {
            Console.WriteLine("Added new Player at: " + endpoint);
            this.players.Add(new Player { X= x, Y=  y, PlayerId = endpoint, Score = 0, Alive = true });
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal List<Ghost> CreateGhosts()
        {
            List<Ghost> ghosts = new List<Ghost>();
            Ghost pink = new Ghost
            {
                X = 200,
                Y = 50,
                Type = GhostType.Pink
            };
            Ghost yellow = new Ghost
            {
                X = 200,
                Y = 235,
                Type = GhostType.Yellow
            };
            Ghost red = new Ghost
            {
                X = 240,
                Y = 90,
                Type = GhostType.Red
            };
            ghosts.Add(pink);
            ghosts.Add(yellow);
            ghosts.Add(red);
            return ghosts;
        }

        internal List<Wall> CreateWalls()
        {
            List<Wall> walls = new List<Wall>();
            Wall w1 = new Wall
            {
                X = 110,
                Y = 50,
                SizeX = 15,
                SizeY = 80
            };
            Wall w2 = new Wall
            {
                X = 280,
                Y = 50,
                SizeX = 20,
                SizeY = 60
            };
            Wall w3 = new Wall
            {
                X = 110,
                Y = 245,
                SizeX = 15,
                SizeY = 80
            };
            Wall w4 = new Wall
            {
                X = 300,
                Y = 245,
                SizeX = 20,
                SizeY = 60,
            };
            walls.Add(w1);
            walls.Add(w2);
            walls.Add(w3);
            walls.Add(w4);
            return walls;
        }

        internal List<Coin> CreateCoins(int totalCoins)
        {
            List<Coin> coins = new List<Coin>();

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
                };

                x += 60;
                columnX++;

                coins.Add(c);
            }
            return coins;
        }

        internal List<Player> CreatePlayers(List<string> clientsEndpoints, int x)
        {
            List<Player> players = new List<Player>();
            int i = 1;
            foreach (var endpoint in clientsEndpoints)
            {
                RegisterPlayer(endpoint, x, i * 40);
                i++;
            }
            return players;
        }

    }
}