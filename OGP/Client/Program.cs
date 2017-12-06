using OGP.Middleware;
using OGP.Server;
using Sprache;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
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
                    // Console.SetOut(new SuppressedWriter());
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrame(argsOptions));

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

        public static string GetHostName(Uri uri)
        {
            return uri.ToString().Replace(uri.PathAndQuery, "");
        }
    }
}