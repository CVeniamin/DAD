using System;
using System.Collections.Generic;

namespace OGP.Server
{
    internal class GameManager
    {
        private event GameTickEventHandler GameTick;

        private ClusterManager clusterManager;
        private List<Game> offeredGames;
        private int tickDuration;
        private int numPlayers;

        public GameManager(ClusterManager clusterManager, List<Game> offeredGames, int tickDuration, int numPlayers)
        {
            this.clusterManager = clusterManager;
            this.offeredGames = offeredGames;
            this.tickDuration = tickDuration;
            this.numPlayers = numPlayers;

            // Handle game creation, teardown, ticks, player joins and leaves
            // this.tick += Game.OnGameTick
            // public event EventHandler ThresholdReached

            // OnGameTick(null);
        }

        protected virtual void OnGameTick(EventArgs e)
        {
            GameTick?.Invoke(e);
        }

        internal void start()
        {
            Console.WriteLine("[GameManager]: {0} game(s) loaded", this.offeredGames.Count);

            Console.WriteLine("[GameManager]: Ready to create rooms for up to {0} player(s)", this.numPlayers);

            Console.WriteLine("[GameManager]: Started ticking every {0}ms", this.tickDuration);
        }
    }
}