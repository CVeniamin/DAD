using System;
using System.Windows.Forms;

namespace OGP.Client
{
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Args received, ready to launch
            // argsOptions.ServiceURL
            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrame(args));
            }
        }
    }
}