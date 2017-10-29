using System;
using System.Collections.Generic;

namespace OGP.Server.Games
{
    internal class Random : Game
    {
        public Random(int tickDuration) : base(tickDuration)
        {
            this.minimumQos = 1;
            this.rules = "This is just a random game";
        }

        internal override IGameState Init()
        {
            Console.WriteLine("Setting up RandomGame");
            return new RandomGameState();
        }
        internal override IGameState Process(IGameState gameState, Dictionary<string, PlayerEvent> playerEventsSnapshot)
        {
            if (gameState.GetType() != typeof(RandomGameState))
            {
                throw new Exception();
            }

            return new RandomGameState();
        }
    }

    class RandomGameState : IGameState
    {
        private bool gameOver;
        public RandomGameState()
        {
            gameOver = false;
        }

        public bool GameOver { get => gameOver; set { gameOver = value;  } }
    }
}