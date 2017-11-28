using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGP.PuppetSlave
{
    class Program
    {
        static void Main(string[] args)
        {

            // Use this to set the process name for server/client
            // Process p = Process.Start("server/client.exe");
            // while (p.MainWindowHandle == IntPtr.Zero)
                // Application.DoEvents();
            // SetWindowText(p.MainWindowHandle, "OGP Server/Client [PID]");
        }
    }

    public interface IHello
    {
        string Hello();
    }
}
