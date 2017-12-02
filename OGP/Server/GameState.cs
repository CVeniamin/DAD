using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGP.Server
{
    class GameStateProxy : MarshalByRefObject
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
    }

    class GameStateView
    {
        private List<GamePlayer> players;
        private List<GameGhost> ghosts;
        private List<GameServer> servers;
        private List<GameCoin> coins;

        public GameStateView()
        {
            
        }
    }

    class GameState
    {
        private GameStateView cachedGameStateView = null;

        public GameState()
        {

        }

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


            cachedGameStateView = gameStateView;

            return true;
        }
    }
}
