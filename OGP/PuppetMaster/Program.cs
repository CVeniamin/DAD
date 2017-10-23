using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OGP.PuppetMaster;

namespace OGP.PuppetMaster
{
    class Program
    {
        static void Main(string[] args)
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
                        bool result = cmd.Exec();
                        Console.WriteLine(result);
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
