using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace OGP.PCS
{
    public class PcsManager : MarshalByRefObject
    {
        private Dictionary<string, Process> processes = null;
        private Dictionary<string, StreamWriter> streamWriters = null;

        public PcsManager()
        {
            processes = new Dictionary<string, Process>();
            streamWriters = new Dictionary<string, StreamWriter>();
        }

        private bool WaitForProcess(Process proc)
        {
            int delaySeconds = 0;
            while (delaySeconds < 5)
            {
                try
                {
                    var time = proc.StartTime;
                    break;
                }
                catch (Exception) { }

                Console.WriteLine("Starting...");
                Thread.Sleep(1000);
                delaySeconds++;

                proc.Refresh();
            }

            return delaySeconds < 5;
        }

        public bool StartServer(string pid, string serverUrl, int msecPerRound, int numPlayers, string serverUrls)
        {
            String args = String.Format("-d -p {0} -u {1} -m {2} -n {3}", pid, serverUrl, msecPerRound, numPlayers);
            if (serverUrls != null)
            {
                args += String.Format(" -s {0}", serverUrls);
            }

            Console.WriteLine("Starting Server with args: {0}", args);

            Process server = new Process();

            server.StartInfo.FileName = "Server.exe";
            server.StartInfo.Arguments = args;
            server.StartInfo.UseShellExecute = false;
            server.StartInfo.RedirectStandardError = true;
            server.StartInfo.RedirectStandardInput = true;
            server.Start();

            bool launchSuccess = WaitForProcess(server);

            if (launchSuccess == true)
            {
                Console.WriteLine("Server Process ready ({0})", server.Id);

                processes.Remove(pid);
                processes.Add(pid, server);

                return true;
            }
            else
            {
                Console.WriteLine("Server process could not start");

                return false;
            }
        }

        public bool StartClient(string pid, string clientURL, int msecPerRound, int numPlayers, string filename, string serverUrls)
        {
            String args = String.Format("-d -p {0} -u {1} -m {2} -n {3}", pid, clientURL, msecPerRound, numPlayers);
            if (filename != null && filename != String.Empty)
            {
                args += String.Format(" -f {0}", filename);
            }
            args += String.Format(" -s {0}", serverUrls);

            Console.WriteLine("Starting Client with args: {0}", args);

            Process client = new Process();

            client.StartInfo.FileName = "Client.exe";
            client.StartInfo.Arguments = args;
            client.StartInfo.UseShellExecute = false;
            client.StartInfo.RedirectStandardError = true;
            client.StartInfo.RedirectStandardInput = true;
            client.Start();

            bool launchSuccess = WaitForProcess(client);

            if (launchSuccess == true)
            {
                Console.WriteLine("Client Process ready ({0})", client.Id);

                processes.Remove(pid);
                processes.Add(pid, client);

                return true;
            }
            else
            {
                Console.WriteLine("Client process could not start");

                return false;
            }
        }

        public string GlobalStatus(string pid)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                Console.WriteLine("Requesting global status from {0}", pid);
                return RequestProcess(process, "GlobalStatus");
            }

            return String.Empty;
        }

        public string LocalState(string pid, int roundId)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                Console.WriteLine("Requesting local state from {0}", pid);
                return RequestProcess(process, String.Format("LocalState {0}", roundId));
            }

            return String.Empty;
        }

        public void InjectDelay(string srcPid, string dstPid)
        {
            if (processes.TryGetValue(srcPid, out Process process))
            {
                process.StandardInput.WriteLine("InjectDelay " + dstPid);
            }
        }

        public void Unfreeze(string pid)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                process.StandardInput.WriteLine("Unfreeze");
            }
        }

        public void Freeze(string pid)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                process.StandardInput.WriteLine("Freeze");
            }
        }

        public void Crash(string pid)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                process.Kill();
            }
        }

        private string RequestProcess(Process process, string request)
        {
            process.StandardInput.WriteLine(request);
            process.StandardInput.Flush();

            StringBuilder output = new StringBuilder();

            string line;
            while ((line = process.StandardError.ReadLine()) != null)
            {
                if (String.IsNullOrEmpty(line))
                {
                    break;
                }
                else
                {
                    output.AppendLine(line);
                }
            }

            return output.ToString();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}