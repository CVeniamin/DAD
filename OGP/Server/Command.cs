using System;

namespace OGP.Server
{
    internal interface ICommand
    {
        string Exec(object arg0);
    }

    internal class GlobalStatus : ICommand
    {
        public string Exec(object arg0)
        {
            // TODO 
            return String.Empty;
        }
    }

    internal class LocalState : ICommand
    {
        private int roundId;

        public LocalState(int roundId)
        {
            this.roundId = roundId;
        }

        public string Exec(object arg0)
        {
            if (arg0 is GameState gameState)
            {
                if (gameState.PreviousGames.TryGetValue(roundId, out string localState))
                {
                    return localState;
                }
            }
            return String.Empty;
        }
    }

    internal class InjectDelay : ICommand
    {
        private string dstPid;

        public InjectDelay(string dstPid)
        {
            // TODO
            this.dstPid = dstPid;
        }

        public string Exec(object arg0)
        {
            return String.Empty;
        }
    }

    internal class Unfreeze : ICommand
    {
        public string Exec(object arg0)
        {
            // TODO
            return String.Empty;
        }
    }

    internal class Freeze : ICommand
    {
        public string Exec(object arg0)
        {
            // TODO
            return String.Empty;
        }
    }
}