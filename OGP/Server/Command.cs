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
            // if server and client are sync

            string masterServer = outManager.GetMasterServer();
            StringBuilder globalStatus = new StringBuilder();

            foreach (Server server in gameState.Servers)
            {
                if(server.Url == masterServer)
                {
                    globalStatus.Append(String.Format("Master server at {0} \n", masterServer));
                }
                else
                {
                    globalStatus.Append(String.Format("Slave server at {0} \n", server.Url));
                }
            }
            globalStatus.Append(String.Format("Number of players {0} \n", gameState.Players.Count));

            foreach (Player player in gameState.Players)
            {
                globalStatus.Append(String.Format("Clients with PID {0} in round {1} at {2} \n", player.PlayerId, gameState.RoundId, player.Url));
            }

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
            outManager.SetDelay(dstPid, 200);
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