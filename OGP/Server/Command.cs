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
            // print if master server
            //print number of clients
            // print roundId for server and client
            // if server and client are sync

            string masterServer = outManager.GetMasterServer();
            StringBuilder output = new StringBuilder();

            foreach (Server server in gameState.Servers)
            {
                if(server.Url == masterServer)
                {
                    output.Append(String.Format("Master server at {0}", masterServer));
                }
            }
            return String.Empty;
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
            // TODO
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