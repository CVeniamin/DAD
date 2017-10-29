using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using RSG;

namespace OGP.Server
{
    internal class GameManager
    {
        private Stopwatch stopwatch;
        private Timer timer;

        private ClusterManager clusterManager;
        private Dictionary<string, Game> offeredGames;
        private int tickDuration;
        private int numPlayers;

        private PromiseTimer promiseTimer;
        private Dictionary<string, GameSession> gameSessions;
        private long lastTickTime;

        public GameManager(ClusterManager clusterManager, ServerDefinition serverDefinition)
        {
            Dictionary<string, Game> offeredGames = new Dictionary<string, Game>();
            foreach (string gameName in serverDefinition.SupportedGames)
            {
                try
                {
                    Type gameType = Type.GetType("OGP.Server.Games." + gameName);
                    offeredGames.Add(gameName, (Game)Activator.CreateInstance(gameType, new object[] { serverDefinition.TickDuration }));
                }
                catch (Exception e)
                {
                    Console.WriteLine("[GameManager] {0} could not be loaded [{1}]", gameName, e.Message);
                }
            }

            this.clusterManager = clusterManager;
            this.offeredGames = offeredGames;
            this.tickDuration = serverDefinition.TickDuration;
            this.numPlayers = serverDefinition.NumPlayers;

            this.gameSessions = new Dictionary<string, GameSession>();

            Console.WriteLine("[GameManager]: {0} game(s) loaded", this.offeredGames.Count);
            Console.WriteLine("[GameManager]: Supporting rooms for up to {0} player(s)", this.numPlayers);
        }

        internal void PlayerJoinedRoom(string gameSessionId, Player player)
        {
            if (gameSessions.TryGetValue(gameSessionId, out GameSession gameSession))
            {
                gameSession.OnPlayerJoin(player);
            }
        }

        internal void PlayerLeftRoom(string gameSessionId, Player player)
        {
            if (gameSessions.TryGetValue(gameSessionId, out GameSession gameSession))
            {
                gameSession.OnPlayerLeave(player);
            }
        }

        public string CreateSession(string gameName)
        {
            if (offeredGames.TryGetValue(gameName, out Game game))
            {
                string gameSessionId = "";
                do
                {
                    gameSessionId = Guid.NewGuid().ToString();
                } while (gameSessions.ContainsKey(gameSessionId));

                if (game.MinimumQOS > 1)
                {
                    // Cluster game session
                }
                else
                {
                    // Single node game session
                    GameSession gameSession = new GameSession(game);
                    gameSessions.Add(gameSessionId, gameSession);

                    Console.WriteLine("[GameManager] GameSession {0} has started", gameSessionId);

                    gameSession.EndPromise.Done(() =>
                        {
                            Console.WriteLine("[GameManager] GameSession {0} has ended", gameSessionId);
                            gameSessions.Remove(gameSessionId);
                            gameSession = null;
                        });
                }

                return gameSessionId;
            }

            return null;
        }

        internal void Exit()
        {
            Console.WriteLine("[GameManager] Exiting...");

            Console.WriteLine("[GameManager] Exited");
        }

        private void OnTimerTick(Object state)
        {
            this.stopwatch.Restart();

            long currentTickTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int deltaTime = (int)(currentTickTime - this.lastTickTime);
            // promiseTimer.Update(currentTickTime - this.lastTickTime);

            foreach (KeyValuePair<string, GameSession> gameSession in gameSessions)
            {
                gameSession.Value.OnGameTick(deltaTime);
            }
            

            this.lastTickTime = currentTickTime;

            this.stopwatch.Stop();
            int processingTime = (int)this.stopwatch.ElapsedMilliseconds;

            int nextTickDelay = this.tickDuration - (processingTime % this.tickDuration);

            if (processingTime > this.tickDuration)
            {
                int skipTicks = processingTime / this.tickDuration;
                Console.WriteLine("[GameManager] Tick processing time exceeded {0}ms limit and took {1}ms, skipping {2} next tick(s)", this.tickDuration, processingTime, skipTicks);
                nextTickDelay += skipTicks * this.tickDuration;
            }

            this.timer.Change(nextTickDelay, Timeout.Infinite);
        }

        internal void Start()
        {
            this.stopwatch = new Stopwatch();
            this.timer = new Timer(OnTimerTick, null, this.tickDuration, Timeout.Infinite);
            this.promiseTimer = new PromiseTimer();

            Console.WriteLine("[GameManager]: Started ticking every {0}ms", this.tickDuration);
        }
    }
}