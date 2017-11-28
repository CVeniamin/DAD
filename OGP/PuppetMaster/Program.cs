using Sprache;
using System;

namespace OGP.PuppetMaster
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("***************************************************");
            Console.WriteLine("*                                                 *");
            Console.WriteLine("* Welcome to PupperMaster                         *");
            Console.WriteLine("* Following commands are available:               *");
            Console.WriteLine("* StartClient, StartServer, GlobalStatus, Crash,  *");
            Console.WriteLine("* Freeze, Unfreeze, Freeze, LocalState, Wait      *");
            Console.WriteLine("* Use Quit to quit                                *");
            Console.WriteLine("*                                                 *");
            Console.WriteLine("***************************************************");

            bool finished = false;
            while (!finished)
            {
                try
                {
                    var input = Console.ReadLine();
                    if (input.Trim() == "Quit")
                    {
                        finished = true;
                    }
                    else
                    {
                        ICommand cmd = CommandParser.Command.Parse(input);
                        string result = cmd.Exec();
                        if (result != String.Empty)
                        {
                            Console.WriteLine(result);
                        }
                        else
                        {
                            Console.WriteLine("OK");
                        }
                    }
                }
                catch (ParseException ex)
                {
                    Console.WriteLine("Wrong command or arguments");
#if DEBUG
                    Console.WriteLine("Error: {0}", ex.Message);
#endif
                }

                Console.WriteLine();
            }
        }
    }
}