using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OGP.Server
{
    public class GameStateProxy : MarshalByRefObject
    {
        private GameState gameState;

        public GameStateProxy(GameState gameState)
        {
            this.gameState = gameState;
        }

        public GameStateView GetGameState()
        {
            return gameState.GetGameState();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    [Serializable]
    public class GameStateView
    {
        private List<GamePlayer> players;
        private List<GameGhost> ghosts;
        private List<GameCoin> coins;
        private List<GameWall> walls;
        private List<GameServer> servers;

        private bool gameStarted;
        private bool gameOver;

        public bool GameStarted { get => gameStarted; set => gameStarted = value; }
        public bool GameOver { get => gameOver; set => gameOver = value; }

        public List<GamePlayer> Players { get => players; set => players = value; }
        public List<GameGhost> Ghosts { get => ghosts; set => ghosts = value; }
        public List<GameCoin> Coins { get => coins; set => coins = value; }
        public List<GameWall> Walls { get => walls; set => walls = value; }
        public List<GameServer> Servers { get => servers; set => servers = value; }

        public GameStateView()
        {
            players = new List<GamePlayer>();
            ghosts = new List<GameGhost>();
            coins = new List<GameCoin>();
            walls = new List<GameWall>();
            servers = new List<GameServer>();
        }

        internal void AddPlayer(GamePlayer gamePlayer)
        {
            players.Add(gamePlayer);
        }

        internal void AddGhost(GameGhost gameGhost)
        {
            ghosts.Add(gameGhost);
        }

        internal void AddCoin(GameCoin gameCoin)
        {
            coins.Add(gameCoin);
        }

        internal void AddWall(GameWall gameWall)
        {
            walls.Add(gameWall);
        }

        internal void AddServer(GameServer gameServer)
        {
            servers.Add(gameServer);
        }
    }

    public class GameState
    {
        private GameStateView cachedGameStateView = null;

        private List<Player> players;
        private List<Ghost> ghosts;
        private List<Coin> coins;
        private List<Wall> walls;
        private List<Server> servers;

        private bool gameStarted;
        private bool gameOver;

        public bool GameStarted { get => gameStarted; set => gameStarted = value; }
        public bool GameOver { get => gameOver; set => gameOver = value; }

        public GameState()
        {
            this.GameStarted = false;
            players = new List<Player>();
            ghosts = new List<Ghost>();
            coins = new List<Coin>();
            walls = new List<Wall>();
            servers = new List<Server>();
        }

        public List<Player> Players { get => players; set => players = value; }
        public List<Ghost> Ghosts { get => ghosts; set => ghosts = value; }
        public List<Coin> Coins { get => coins; set => coins = value; }
        public List<Wall> Walls { get => walls; set => walls = value; }
        public List<Server> Servers { get => servers; set => servers = value; }
        

        internal GameStateView GetGameState()
        {
            if (cachedGameStateView == null && GenerateGameStateView())
            {
                return cachedGameStateView; // Return newly generated state
            }

            return cachedGameStateView; // Return null or cached game state
        }

        private bool GenerateGameStateView()
        {
            GameStateView gameStateView = new GameStateView();

            foreach (Player player in players)
            {
               gameStateView.AddPlayer(new GamePlayer(player.X, player.Y, player.PlayerId, player.Score, player.Alive));
            }

            foreach (Ghost ghost in ghosts)
            {
                gameStateView.AddGhost(new GameGhost(ghost.X, ghost.Y, ghost.Type));
            }

            foreach (Coin coin in coins)
            {
                gameStateView.AddCoin(new GameCoin(coin.X, coin.Y));
            }

            foreach (Wall wall in walls)
            {
                gameStateView.AddWall(new GameWall(wall.X, wall.Y, wall.SizeX, wall.SizeY));
            }

            foreach (Server server in servers)
            {
                gameStateView.AddServer(new GameServer(server.Url));
            }
            cachedGameStateView = gameStateView;

            return true;
        }
    }
}