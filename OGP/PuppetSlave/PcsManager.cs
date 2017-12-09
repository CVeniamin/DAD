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
            String args;
            if (serverUrls == null)
            {
                args = "-d -p " + pid + " -u " + serverUrl + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();
            }
            else
            {
                args = "-d -p " + pid + " -u " + serverUrl + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString() + " -s " + serverUrls;
            }

            Console.WriteLine("Starting Server with args: " + args);
            //use this way of starting Server.exe
            //Process server = Process.Start("Server.exe", args);

            // or use this way
            Process server = new Process();

            server.StartInfo.FileName = "Server.exe";
            server.StartInfo.Arguments = args;
            server.StartInfo.UseShellExecute = false;
            //server.StartInfo.RedirectStandardOutput = true;
            server.StartInfo.RedirectStandardInput = true;
            server.Start();

            bool launchSuccess = WaitForProcess(server);

            if (launchSuccess == true)
            {
                Console.WriteLine("Server Process ready ({0})", server.Id);
                processes.Add(pid, server);
                streamWriters.Add(pid, server.StandardInput);

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
            String args = "-d -p " + pid + " -u " + clientURL + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();

            if (filename != null && filename != String.Empty)
            {
                args += " -f " + filename;
            }

            args += " -s " + serverUrls;

            Console.WriteLine("Starting Client with args: " + args);

            Process client = new Process();

            client.StartInfo.FileName = "Client.exe";
            client.StartInfo.Arguments = args;
            client.StartInfo.UseShellExecute = false;
            client.StartInfo.RedirectStandardInput = true;
            client.StartInfo.RedirectStandardOutput = true;
            client.Start();

            bool launchSuccess = WaitForProcess(client);

            if (launchSuccess == true)
            {
                Console.WriteLine("Client Process ready ({0})", client.Id);

                //processes.Remove(pid); // Prevent error for duplicate key
                // TODO: detect child process crash and remove from process list

                if (!processes.TryGetValue(pid, out Process p))
                {
                    processes.Add(pid, client);
                }

                if (!streamWriters.TryGetValue(pid, out StreamWriter sw))
                {
                    streamWriters.Add(pid, client.StandardInput);
                }

                return true;
            }
            else
            {
                Console.WriteLine("Client process could not start");

                return false;
            }
        }

        public void GlobalStatus(string pid)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                Console.WriteLine("Requesting global status from " + pid);

                process.StandardInput.WriteLine("GlobalStatus");

                string line;
                process.BeginOutputReadLine();
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }

        private StringBuilder output = null;
        public string LocalState(string pid, int roundId)
        {
            // TODO: check if this while returns something since client continuously will sends stuff to console and will never return.
            if (processes.TryGetValue(pid, out Process process))
            {
                Console.WriteLine("Requesting local state from " + pid);

                //process.StandardInput.WriteLine("LocalState " + roundId);
                if (streamWriters.TryGetValue(pid, out StreamWriter streamWriter))
                {
                    //process.StandardInput.WriteLine("LocalState " + roundId);
                    streamWriter.WriteLine("LocalState " + roundId);
                    streamWriter.Close();
                }

                output = new StringBuilder();

                process.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                process.BeginOutputReadLine();

                Console.WriteLine("Before wait for exit!");

                //process.WaitForExit();
                //process.Close();
                return output.ToString();
            }
            return String.Empty;
        }

        private void SortOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                //output.Append(outLine.Data);
                Console.WriteLine(outLine.Data);
            }
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

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}