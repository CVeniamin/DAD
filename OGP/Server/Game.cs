﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
        String URL { get; set; }
        public GameClient(String url)
        {
            this.URL = url;
        }
    }

    public class Game : MarshalByRefObject, IGame
    {
        private List<GameClient> gameClients;
        private int tickDuration;
        private short minimumPlayers = 1;
        private int gameID;
        
        public List<GameClient> GameClients { get => gameClients; set => gameClients = value; }
        public int TickDuration { get => tickDuration; set => tickDuration = value; }
        public short MinimumPlayers { get; set; }
        public int GameID { get => gameID; set => gameID = value; }

        public Game() { }

        public Game(int tickDuration, short minimumPlayers, int gameID)
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
    }
}