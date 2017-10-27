using System;

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

        internal override void OnPlayerJoin(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void OnPlayerLeave(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void OnGameTick(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Setup(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Sart(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void End(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Teardown(EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}