using System;
using System.Windows.Forms;

namespace OGP.Client
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Args received, ready to launch
            // argsOptions.ServiceURL
            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                if (argsOptions.PCS == "1")
                {
                    Console.SetOut(new SuppressedWriter());
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrame(args));
            }
        }
    }
}