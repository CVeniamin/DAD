using System;

namespace OGP.Server
{
    public delegate void PlayerJoinEventHandler(EventArgs e);

    public delegate void PlayerLeaveEventHandler(EventArgs e);

    public delegate void GameTickEventHandler(EventArgs e);

    internal abstract class Game
    {
        private int tickDuration;

        public Game(int tickDuration)
        {
            this.tickDuration = tickDuration;
        }

        internal abstract void OnPlayerJoin(EventArgs e);

        internal abstract void OnPlayerLeave(EventArgs e);

        internal abstract void OnGameTick(EventArgs e);

        internal abstract void Setup(EventArgs e);

        internal abstract void Sart(EventArgs e);

        internal abstract void End(EventArgs e);

        internal abstract void Teardown(EventArgs e);
    }
}