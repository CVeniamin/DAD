using System;
using System.Collections.Generic;

namespace OGP.Server.Games
{
    internal class Pacman : Game
    {
        public event EventHandler ThresholdReached;

        public Pacman(int tickDuration) : base(tickDuration)
        {
            // initialize game funcions and structures
        }

        protected virtual void OnThresholdReached(EventArgs e)
        {
            EventHandler handler = ThresholdReached;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal override IGameState Init()
        {
            throw new NotImplementedException();
        }

        internal override IGameState Process(IGameState gameState, Dictionary<string, PlayerEvent> playerEventsSnapshot)
        {
            throw new NotImplementedException();
        }
    }
}