using System;
using System.Collections.Generic;

namespace OGP.Server
{
    public interface IGame
    {
        bool GameOver { get; set; }

        void Init(int numOfCoins);

        void MoveLeft();

        void MoveRight();

        void MoveUp();

        void MoveDown();

        void RegisterClients(List<string> clientsEndpoints);
    }

    public interface IGameService
    {
        bool AddGame(Game g);
    }

    [Serializable]
    public class GameClient
    {
        private String URL { get; set; }

        public GameClient(String url)
        {
            this.URL = url;
        }
    }

    public class Game : MarshalByRefObject, IGame
    {
        private List<GameClient> gameClients;
        private int tickDuration;
        private int minimumPlayers = 1;
        private int gameID;
        private bool gameOver;
        private bool gameStarted;
        private GameState gameState;

        public List<GameClient> GameClients { get => gameClients; set => gameClients = value; }
        public int TickDuration { get => tickDuration; set => tickDuration = value; }
        public int MinimumPlayers { get; set; }
        public int GameID { get => gameID; set => gameID = value; }
        public bool GameOver { get => gameOver; set => gameOver = value; }
        public bool GameStarted { get => gameStarted; set => gameStarted = value; }


        public Game()
        {
        }

        public Game(GameState state, int tickDuration, int minimumPlayers, int gameID)
        {
            this.gameState = state;
            this.gameStarted = false;
            this.gameOver = false;
            this.tickDuration = tickDuration;
            this.minimumPlayers = minimumPlayers;
            this.gameID = gameID;
            this.gameClients = new List<GameClient>();
        }

        public void Init(int numOfCoins)
        {
            if(gameState != null)
            {
                //gameState.Players = 
                gameState.Walls = CreateWalls();
                gameState.Coins = CreateCoins(numOfCoins);
                gameState.Ghosts = CreateGhosts();
            }
        }

        public void MoveLeft()
        {
            throw new NotImplementedException();
        }

        public void MoveRight()
        {
            throw new NotImplementedException();
        }

        public void MoveUp()
        {
            throw new NotImplementedException();
        }

        public void MoveDown()
        {
            throw new NotImplementedException();
        }

        public void RegisterClients(List<string> clientsEndpoints)
        {
            foreach(var endpoint in clientsEndpoints)
            {
                this.GameClients.Add(new GameClient(endpoint));
                Console.WriteLine("Added new client at: " + endpoint);
            }
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
    }
}