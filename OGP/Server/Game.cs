using System;
using System.Collections.Generic;

namespace OGP.Server
{
    public interface IGame
    {
        bool GameOver { get; set; }

        void Init();

        void MoveLeft();

        void MoveRight();

        void MoveUp();

        void MoveDown();

        void RegisterClient(string url);
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

        public List<GameClient> GameClients { get => gameClients; set => gameClients = value; }
        public int TickDuration { get => tickDuration; set => tickDuration = value; }
        public int MinimumPlayers { get; set; }
        public int GameID { get => gameID; set => gameID = value; }

        public Game()
        {
        }

        public Game(int tickDuration, int minimumPlayers, int gameID)
        {
            this.tickDuration = tickDuration;
            this.minimumPlayers = minimumPlayers;
            this.gameID = gameID;
            this.gameClients = new List<GameClient>();
        }

        public bool GameOver { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string SayHello()
        {
            return "Hello";
        }

        public void Init()
        {
            throw new NotImplementedException();
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

        public void RegisterClient(string url)
        {
            this.GameClients.Add(new GameClient(url));
            Console.WriteLine("Added new client at: " + url);
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
    }
}