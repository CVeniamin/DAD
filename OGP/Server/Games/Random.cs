using System;

namespace OGP.Server.Games
{
    internal class Random : Game
    {
        public Random(int tickDuration) : base(tickDuration)
        {
        }

        internal override void End(EventArgs e)
        {
            throw new NotImplementedException();
        }
        
        internal override void OnGameTick(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void OnPlayerJoin(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void OnPlayerLeave(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Sart(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Setup(EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Teardown(EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}