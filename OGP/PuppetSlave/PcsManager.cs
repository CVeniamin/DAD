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

        // Use this to set the process name for server/client
        // Process p = Process.Start("server/client.exe");
        // while (p.MainWindowHandle == IntPtr.Zero)
        // Application.DoEvents();
        // SetWindowText(p.MainWindowHandle, "OGP Server/Client [PID]");

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);
        
        public void StartServer(string pid, string serverUrl, int msecPerRound, int numPlayers)
        {
            String args = "-p " + pid + " -s " + serverUrl + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();
            
            Console.WriteLine("Starting Server with args: " + args);
            Process process = Process.Start("Server.exe", args);

            Task.Delay(1000).ContinueWith(t =>
            {
                while (process.MainWindowHandle == IntPtr.Zero)
                    Thread.Sleep(1000);
                SetWindowText(process.MainWindowHandle, "OGP Server " + pid);
            });

        }

        public void StartClient(string pid, string clientURL, int msecPerRound, int numPlayers, string filename, string serverUrls)
        {
            String args = "-p " + pid + " -c " + clientURL + " -m " + msecPerRound.ToString() + " -n " + numPlayers.ToString();

            if (filename != null && filename != String.Empty)
            {
                args += " -f " + filename;
            }
            
            args += " -s " + serverUrls;

            Console.WriteLine("Starting Client with args: " + args);
            Process process = Process.Start("Client.exe", args);

            Task.Delay(1000).ContinueWith(t =>
            {
                while (process.MainWindowHandle == IntPtr.Zero)
                    Thread.Sleep(1000);
                SetWindowText(process.MainWindowHandle, "OGP Client " + pid);
            });

        }
        
        public void PrintStatus(string pid)
        {

            Console.WriteLine("Requesting status print from " + pid);
        }

        public void InjectDelay(string srcPid, string dstPid)
        {
            throw new NotImplementedException();
        }

        public void Unfreeze(string pid)
        {
            throw new NotImplementedException();
        }

        public void Freeze(string pid)
        {
            throw new NotImplementedException();
        }

        public void Crash(string pid)
        {
            throw new NotImplementedException();
        }
    }
}
