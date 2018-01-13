using OGP.Middleware;
using System;
using System.Text;

namespace OGP.Server
{
    public interface ICommand
    {
        string Exec(GameState gameState, InManager inManager, OutManager outManager);
    }

    public class GlobalStatus : ICommand
    {
        public string Exec(GameState gameState, InManager inManager, OutManager outManager)
        {
            string masterServer = outManager.GetMasterServer();

            StringBuilder globalStatus = new StringBuilder();

            if (gameState.RoundId == -1)
            {
                globalStatus.AppendLine(String.Format("Game waiting for players"));
            }
            else if (gameState.GameOver)
            {
                Player winner = null;
                foreach (Player player in gameState.Players)
                {
                    if (player.Score > winner.Score)
                    {
                        winner = player;
                    }
                }

                if (winner != null)
                {
                    globalStatus.AppendLine(String.Format("Game over. {0} won after {1} rounds", winner.PlayerId, gameState.RoundId));
                }
                else
                {
                    globalStatus.AppendLine(String.Format("Game over after {0} rounds", gameState.RoundId));
                }
            }
            else
            {
                globalStatus.AppendLine(String.Format("Game live. Round {0}", gameState.RoundId));
            }

            globalStatus.AppendLine("Servers:");
            foreach (GameServer server in gameState.Servers)
            {
                globalStatus.AppendLine(String.Format("{0} [{1}]", server.Url, server.Url == masterServer ? "MASTER" : "SLAVE"));
            }

            globalStatus.AppendLine("Players:");
            globalStatus.AppendLine("PID\tScore\tAlive");
            foreach (Player player in gameState.Players)
            {
                globalStatus.AppendLine(String.Format("{0}\t{1}\t{2}", player.PlayerId, player.Score, player.Alive));
            }

            globalStatus.AppendLine(String.Format("# Players: {0}\n", gameState.Players.Count));

            return globalStatus.ToString();
        }
    }

    public class LocalState : ICommand
    {
        private int roundId;

        public LocalState(int roundId)
        {
            this.roundId = roundId;
        }

        public string Exec(GameState gameState, InManager inManager, OutManager outManager)
        {
            if (gameState.PreviousGames.TryGetValue(roundId, out string localState))
            {
                return localState;
            }
            return String.Empty;
        }
    }

    public class InjectDelay : ICommand
    {
        private string dstPid;

        public InjectDelay(string dstPid)
        {
            this.dstPid = dstPid;
        }

        public string Exec(GameState gameState, InManager inManager, OutManager outManager)
        {
            outManager.SetDelay(dstPid, 10000);
            return String.Empty;
        }
    }

    public class Unfreeze : ICommand
    {
        public string Exec(GameState gameState, InManager inManager, OutManager outManager)
        {
            inManager.Unfreeze();
            return String.Empty;
        }
    }

    public class Freeze : ICommand
    {
        public string Exec(GameState gameState, InManager inManager, OutManager outManager)
        {
            inManager.Freeze();
            return String.Empty;
        }
    }
}