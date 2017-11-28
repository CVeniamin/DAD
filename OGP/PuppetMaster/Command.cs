using OGP.PCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace OGP.PuppetMaster
{
    internal interface ICommand
    {
        bool Exec();
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

        public bool Exec()
        {
            PcsManager pcs = PcsPool.GetByUrl(pcsUrl);

            string serverURLs = "";
            if (ServerList.Servers.Count > 0)
            {
                foreach (string serverUrl in ServerList.Servers)
                {
                    serverURLs += serverUrl + ",";
                }
            }

            pcs.StartClient(pid, clientUrl, msecPerRound, numPlayers, filename, serverURLs);
            PcsPool.LinkPid(pid, pcsUrl);

            return true;
        }
    }

    internal class StartServer : ICommand
    {
        private String pid;
        private String pcsUrl;
        private String serverUrl;
        private int msecPerRound;
        private int numPlayers;
        private String filename;

        public StartServer(string pid, string pcsUrl, string serverUrl, int msecPerRound, int numPlayers)
        {
            this.pid = pid;
            this.pcsUrl = pcsUrl;
            this.serverUrl = serverUrl;
            this.msecPerRound = msecPerRound;
            this.numPlayers = numPlayers;
        }

        public bool Exec()
        {
            PcsManager pcs = PcsPool.GetByUrl(pcsUrl);
            
            pcs.StartServer(pid, serverUrl, msecPerRound, numPlayers);

            ServerList.AddServer(serverUrl);
            PcsPool.LinkPid(pid, pcsUrl);

            return true;
        }
    }

    internal class GlobalStatus : ICommand
    {
        public GlobalStatus()
        {
        }

        public bool Exec()
        {
            foreach (string pid in PcsPool.GetAllPids())
            {
                PcsManager pcs = PcsPool.GetByPid(pid);
                if (pcs != null)
                {
                    pcs.PrintStatus(pid);
                }
            }

            return true;
        }
    }

    internal class Crash : ICommand
    {
        private string pid;

        public Crash(string pid)
        {
            this.pid = pid;
        }

        public bool Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                pcs.Crash(pid);
            }

            return true;
        }
    }

    internal class Freeze : ICommand
    {
        private string pid;

        public Freeze(string pid)
        {
            this.pid = pid;
        }

        public bool Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                pcs.Freeze(pid);
            }

            return true;
        }
    }

    internal class Unfreeze : ICommand
    {
        private string pid;

        public Unfreeze(string pid)
        {
            this.pid = pid;
        }

        public bool Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(pid);
            if (pcs != null)
            {
                pcs.Unfreeze(pid);
            }

            return true;
        }
    }

    internal class InjectDelay : ICommand
    {
        string srcPid;
        string dstPid;

        public InjectDelay(string srcPid, string dstPid)
        {
            this.srcPid = srcPid;
            this.dstPid = dstPid;
        }

        public bool Exec()
        {
            PcsManager pcs = PcsPool.GetByPid(srcPid);
            if (pcs != null)
            {
                pcs.InjectDelay(srcPid, dstPid);
            }

            return true;
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
        int ms;
        public Wait(int ms)
        {
            this.ms = ms;
        }

        public bool Exec()
        {
            Thread.Sleep(ms);

            return true;
        }
    }
}