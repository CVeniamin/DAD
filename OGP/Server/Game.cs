using System;
using System.Collections.Generic;

namespace OGP.Server
{
    internal abstract class Game
    {
        private int tickDuration;

        protected int minimumQos = 1;
        protected int minimumPlayers = 1;
        protected string rules = "";

        public int MinimumQOS { get { return minimumQos;  } }
        public int MinimumPlayers { get { return minimumPlayers;  } }
        public string Rules { get { return rules; } }

        public Game(int tickDuration)
        {
            this.tickDuration = tickDuration;
        }

        internal abstract IGameState Init();
        internal abstract IGameState Process(IGameState gameState, Dictionary<string, PlayerEvent> playerEventsSnapshot);
    }

    internal interface IGameState
    {
        bool GameOver { get; set; }
    }
}