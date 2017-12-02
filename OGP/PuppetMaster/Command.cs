using OGP.PCS;
using System;
using System.Threading;

namespace OGP.PuppetMaster
{
    internal interface ICommand
    {
        string Exec();
    }

    internal class StartClient : ICommand
    {
        private String pid;
        private String pcsUrl;
        private String clientUrl;
        private int msecPerRound;
        private int numPlayers;
        private String filename;

        public StartClient(string pid, string pcsUrl, string clientUrl, int msecPerRound, int numPlayers, string filename)
        {
            this.pid = pid;
            this.pcsUrl = pcsUrl;
            this.clientUrl = clientUrl;
            this.msecPerRound = msecPerRound;
            this.numPlayers = numPlayers;
            this.filename = filename;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByUrl(pcsUrl);

            string serverURLs = String.Join(",", ServerList.Servers);

            bool result = pcs.StartClient(pid, clientUrl, msecPerRound, numPlayers, filename, serverURLs);

            if (result == true)
            {
                PcsPool.LinkPid(pid, pcsUrl);

                return String.Empty;
            }
            else
            {
                return "Error - process not started";
            }
        }
    }

    internal class StartServer : ICommand
    {
        private String pid;
        private String pcsUrl;
        private String serverUrl;
        private int msecPerRound;
        private int numPlayers;

        public StartServer(string pid, string pcsUrl, string serverUrl, int msecPerRound, int numPlayers)
        {
            this.pid = pid;
            this.pcsUrl = pcsUrl;
            this.serverUrl = serverUrl;
            this.msecPerRound = msecPerRound;
            this.numPlayers = numPlayers;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByUrl(pcsUrl);

            string serverURLs = String.Join(",", ServerList.Servers);

            bool result = pcs.StartServer(pid, serverUrl, msecPerRound, numPlayers, serverURLs);

            if (result == true)
            {
                ServerList.AddServer(serverUrl);
                PcsPool.LinkPid(pid, pcsUrl);

                return String.Empty;
            }
            else
            {
                return "Error - process not started";
            }
        }
    }

    internal class GlobalStatus : ICommand
    {
        public GlobalStatus()
        {
        }

        public string Exec()
        {
            foreach (string pid in PcsPool.GetAllPids())
            {
                PcsManager pcs = PcsPool.GetByPid(pid);
                if (pcs != null)
                {
                    pcs.GlobalStatus(pid);
                }
            }

            return String.Empty;
        }
    }

    internal class Crash : ICommand
    {
        private string pid;

        public Crash(string pid)
        {
            this.pid = pid;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                pcs.Crash(pid);
            }

            return String.Empty;
        }
    }

    internal class Freeze : ICommand
    {
        private string pid;

        public Freeze(string pid)
        {
            this.pid = pid;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                pcs.Freeze(pid);
            }

            return String.Empty;
        }
    }

    internal class Unfreeze : ICommand
    {
        private string pid;

        public Unfreeze(string pid)
        {
            this.pid = pid;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                pcs.Unfreeze(pid);
            }

            return String.Empty;
        }
    }

    internal class InjectDelay : ICommand
    {
        private string srcPid;
        private string dstPid;

        public InjectDelay(string srcPid, string dstPid)
        {
            this.srcPid = srcPid;
            this.dstPid = dstPid;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(srcPid);
            if (pcs != null)
            {
                pcs.InjectDelay(srcPid, dstPid);
            }

            return String.Empty;
        }
    }

    internal class LocalState : ICommand
    {
        private string pid;
        private int roundId;

        public LocalState(string pid, int roundId)
        {
            this.pid = pid;
            this.roundId = roundId;
        }

        public string Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                return pcs.LocalState(pid, roundId);
            }

            return String.Empty;
        }
    }

    internal class Wait : ICommand
    {
        private int ms;

        public Wait(int ms)
        {
            this.ms = ms;
        }

        public string Exec()
        {
            Thread.Sleep(ms);

            return String.Empty;
        }
    }
}