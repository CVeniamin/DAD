using OGP.Server;
using System;

namespace OGP.Client
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
            // TODO
            if (arg0 is GameState gameState)
            {
                Console.WriteLine("Called Exec on LocalState", "CRITICAL");
                string output = gameState.WriteState();
                Console.WriteLine("output " + output, "CRITICAL");
                return output;
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