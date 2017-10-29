using System;
using System.Collections.Generic;
using RSG;

namespace OGP.Server
{
    public delegate void GameSessionTickEventHandler(int tickId);
    
    internal class GameSession
    {
        private Game game;
        private IGameState gameState;
        
        private bool isGameRunning = false;
        private int tickId = 0;
        private PromiseTimer gameTimer;
        private PromiseTimer playerTimer;
        private Promise endPromise;

        private Dictionary<string, Player> players;
        private Dictionary<string, PlayerEvent> playerEvents;


        public GameSession(Game game)
        {
            this.game = game;
            this.gameState = game.Init();

            this.players = new Dictionary<string, Player>();
            this.playerEvents = new Dictionary<string, PlayerEvent>();

            this.gameTimer = new PromiseTimer();
            this.playerTimer = new PromiseTimer();

            this.endPromise = new Promise();

            WaitForMinimumPlayers() // Preparation phase = wait for enough players to join
                .Then(() => RunGame()) // Active phase = process ticks, passing the state to game and then sending results back
                .Then(() => SessionOver()) // End phase = send winner, save game state to disk, etc.
                .Then(() => endPromise.Resolve())
                .Done();
        }

        internal void OnGameTick(int deltaTime)
        {
            if (this.isGameRunning)
            {
                this.tickId++;
                this.gameTimer.Update(deltaTime);
            }
        }

        internal void OnPlayerJoin(Player player)
        {
            if (players.ContainsKey(player.PlayerId))
            {
                return;
            }

            players.Add(player.PlayerId, player);

            playerTimer.Update(0);
        }

        internal void OnPlayerLeave(Player player)
        {
            if (players.Remove(player.PlayerId))
            {
                playerTimer.Update(0);
            }
        }

        internal void OnPlayerEvent(string playerId, PlayerEvent playerEvent)
        {
            if (players.ContainsKey(playerId))
            {
                playerEvents.Add(playerId, playerEvent);
            }
        }

        private IPromise WaitForMinimumPlayers()
        {
            Console.WriteLine("WaitForMinimumPlayers -> waiting until enough players join");

            return this.playerTimer.WaitUntil(_ =>
            {
                Console.WriteLine("Checking if enough players to start {0}/{1}", players.Count, this.game.MinimumPlayers);
                return players.Count >= this.game.MinimumPlayers;
            }
            );
        }

        private IPromise RunGame()
        {
            Console.WriteLine("RunGame -> starting processing");

            this.isGameRunning = true;

            return Promise.Race(this.GameOver(), this.AllPlayersLeft())
                .Then(() => {
                    this.isGameRunning = false;

                    return Promise.Resolved();
                });
        }
        
        private IPromise GameOver()
        {
            return this.gameTimer.WaitUntil(t =>
            {
                Console.Write(".");

                // Process tick here
                Dictionary<string, PlayerEvent> playerEventsSnapshot = new Dictionary<string, PlayerEvent>(this.playerEvents);
                this.playerEvents.Clear();

                this.gameState = game.Process(this.gameState, playerEventsSnapshot);
                
                return gameState.GameOver;
            });
        }
        
        private IPromise AllPlayersLeft()
        {
            return this.playerTimer.WaitUntil(_ =>
            {
                return players.Count == 0;
            }
            );
        }

        private IPromise SessionOver()
        {
            Console.WriteLine("SessionOver -> notify of winner and such");
            return Promise.Resolved();
        }

        public IPromise EndPromise => endPromise;
    }
}