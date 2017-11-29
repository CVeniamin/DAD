using System;
using System.Windows.Forms;

namespace OGP.Client
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Console.WriteLine(argsOptions.Pcs);

                if (argsOptions.Pcs != null)
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