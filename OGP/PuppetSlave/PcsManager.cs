using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OGP.PCS
{
    public class PcsManager : MarshalByRefObject
    {
        private Dictionary<string, Process> processes = null;

        public PcsManager()
        {
            processes = new Dictionary<string, Process>();
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);

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

        public bool StartServer(string pid, string serverUrl, int msecPerRound, int numPlayers)
        {
            String args = "-d -pp " + pid + " -s " + serverUrl + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();

            Console.WriteLine("Starting Server with args: " + args);

            Process server = new Process();

            server.StartInfo.FileName = "Server.exe";
            server.StartInfo.Arguments = args;
            server.StartInfo.UseShellExecute = false;
            server.StartInfo.RedirectStandardOutput = true;
            server.Start();

            bool launchSuccess = WaitForProcess(server);

            if (launchSuccess == true)
            {
                Console.WriteLine("Server Process ready ({0})", server.Id);
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
            String args = "--pcs -p " + pid + " -c " + clientURL + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();

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
            client.StartInfo.RedirectStandardOutput = true;
            client.Start();

            bool launchSuccess = WaitForProcess(client);

            if (launchSuccess == true)
            {
                Console.WriteLine("Client Process ready ({0})", client.Id);

                processes.Remove(pid); // Prevent error for duplicate key
                // TODO: detect child process crash and remove from process list
                processes.Add(pid, client);

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

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }

        public string LocalState(string pid, int roundId)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                Console.WriteLine("Requesting local state from " + pid);

                process.StandardInput.WriteLine("LocalState " + roundId);

                string line;
                string output = "";

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (line == String.Empty)
                    {
                        return output;
                    }
                    else
                    {
                        output += line + "\n";
                    }
                }

                return output;
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

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}