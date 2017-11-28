using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        
        public void StartServer(string pid, string serverUrl, int msecPerRound, int numPlayers)
        {
            String args = "--pcs -p " + pid + " -s " + serverUrl + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();
            
            Console.WriteLine("Starting Server with args: " + args);

            Process server = new Process();

            server.StartInfo.FileName = "Server.exe";
            server.StartInfo.Arguments = args;
            server.StartInfo.UseShellExecute = false;
            server.StartInfo.RedirectStandardOutput = true;
            server.Start();
            
            Task.Delay(1000).ContinueWith(t =>
            {
                while (server.MainWindowHandle == IntPtr.Zero)
                    Thread.Sleep(1000);
                SetWindowText(server.MainWindowHandle, "OGP Server " + pid);
            });

        }

        public void StartClient(string pid, string clientURL, int msecPerRound, int numPlayers, string filename, string serverUrls)
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
            
            Task.Delay(1000).ContinueWith(t =>
            {
                while (client.MainWindowHandle == IntPtr.Zero)
                    Thread.Sleep(1000);
                SetWindowText(client.MainWindowHandle, "OGP Client " + pid);
            });

            processes.Add(pid, client);
        }
        
        public void PrintStatus(string pid)
        {
            if (processes.TryGetValue(pid, out Process process))
            {
                Console.WriteLine("Requesting status print from " + pid);

                process.StandardInput.WriteLine("PrintStatus");
                
                string line;
                string output = "";

                while (( line = process.StandardOutput.ReadLine()) != null) {
                    if (line == String.Empty)
                    {
                        // TODO: Send data to PM
                        break;
                    }
                    else
                    {
                        output += line + "\n";
                    }
                }
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
