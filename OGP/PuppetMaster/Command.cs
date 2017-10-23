using System;

namespace OGP.PuppetMaster
{
    internal interface ICommand
    {
        bool Exec();
    }

    internal class StartClient : ICommand
    {
        public StartClient(string pid, string pcsURL, string clientURL, int msecPerRound, int numPlayers, string filename)
        {
        }

        public bool Exec()
        {
            return false;
        }
    }

    internal class StartServer : ICommand
    {
        public StartServer(string pid, string pcsURL, string serverURL, int msecPerRound, int numPlayers)
        {
        }

        public bool Exec()
        {
            return false;
        }
    }

    internal class GlobalStatus : ICommand
    {
        public GlobalStatus()
        {
        }

        public bool Exec()
        {
            return false;
        }
    }

    internal class Crash : ICommand
    {
        public Crash(string pid)
        {
        }

        public bool Exec()
        {
            return false;
        }
    }

    internal class Freeze : ICommand
    {
        private string _pid;

        public Freeze(string pid)
        {
            _pid = pid;
        }

        public bool Exec()
        {
            Console.WriteLine("Freezing {$0}", _pid);
            return false;
        }
    }

    internal class Unfreeze : ICommand
    {
        private string _pid;

        public Unfreeze(string pid)
        {
            _pid = pid;
        }

        public bool Exec()
        {
            Console.WriteLine("Unfreezing {$0}", _pid);
            return false;
        }
    }

    internal class InjectDelay : ICommand
    {
        public InjectDelay(string srcPid, string dstPid)
        {
        }

        public bool Exec()
        {
            return false;
        }
    }

    internal class LocalState : ICommand
    {
        public LocalState(string pid, int roundID)
        {
        }

        public bool Exec()
        {
            return false;
        }
    }

    internal class Wait : ICommand
    {
        public Wait(long ms)
        {
        }

        public bool Exec()
        {
            return false;
        }
    }
}