using Sprache;
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

                if (argsOptions.Pcs == true)
                {
                    Console.WriteLine("Suppressing output");
                    Console.SetOut(new SuppressedWriter());
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrame(args));

                // Start listening for input
                while (true)
                {
                    var input = Console.ReadLine();

                    if (input == null || input.Trim() == "Quit")
                    {
                        Console.WriteLine("Exit triggered by input", "CRITICAL");
                        break;
                    }

                    ICommand cmd = CommandParser.Command.Parse(input);
                    cmd.Exec(null); // TODO: dependency injection to allow grabbing state and such
                }
            }
            else
            {
                Console.WriteLine("Missing required arguments");
            }
        }
    }
}