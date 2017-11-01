using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OGP.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            // Args received, ready to launch
            // argsOptions.ServiceURL

            //var argsOptions = new ArgsOptions();
            //if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            //{

            //}

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainFrame());

        }
    }
}
