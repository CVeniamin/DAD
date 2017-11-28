using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OGP.PuppetMaster
{
    internal interface ICommand
    {
        bool Exec();
    }

    public class Endpoint
    {
        private static List<string> servers = null;

        public Endpoint(string url)
        {
            if (servers == null)
            {
                servers = new List<string>();
            }
            servers.Add(url);
        }

        public static List<string> Servers { get => servers; set => servers = value; }
    }

    internal class StartClient : ICommand
    {
        private String args;

        public StartClient(string pid, string pcsURL, string clientURL, int msecPerRound, int numPlayers, string filename)
        {
            args = "-p " + pid + " -u " + pcsURL + " -c " + clientURL + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();

            if (!(filename == null || filename == String.Empty))
            {
                args += " -f " + filename;
            }
            if (Endpoint.Servers.Count > 0)
            {
                args += " -s ";
                foreach (var serverURL in Endpoint.Servers)
                {
                    args += serverURL + ",";
                }
            }
        }

        public bool Exec()
        {
            Process.Start("Client.exe", args);
            Console.WriteLine(args);
            return true;
        }
    }

    internal class StartServer : ICommand
    {
        private String args;

        public StartServer(string pid, string pcsURL, string serverURL, int msecPerRound, int numPlayers)
        {
            args = "-p " + pid + " -u " + pcsURL + " -s " + serverURL + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();
            new Endpoint(serverURL);
        }

        public bool Exec()
        {
            Process.Start("Server.exe", args);
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
            Console.WriteLine("Freezing {0}", _pid);
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
            Console.WriteLine("Unfreezing {0}", _pid);
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