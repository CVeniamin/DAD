using Sprache;
using System;
using System.IO;
using System.Net.Sockets;

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
            Console.WriteLine("* Use Quit to quit.                               *");
            Console.WriteLine("*                                                 *");
            Console.WriteLine("***************************************************");

            bool finished = false;
            while (!finished)
            {
                try
                {
                    var input = Console.ReadLine();
                    if (input == null || input.Trim() == "Quit")
                    {
                        finished = true;
                    }
                    else
                    {
                        ICommand cmd = CommandParser.Command.Parse(input);
                        try
                        {
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
                        catch (Exception ex)
                        {
                            if (ex is IOException || ex is SocketException)
                            {
                                Console.WriteLine("Command execution failed. Connection to PCS lost. Exiting.");
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Command execution error.");
                            }
                        }
                    }
                }
                catch (ParseException ex)
                {
                    Console.WriteLine("Wrong command or arguments.");
#if DEBUG
                    Console.WriteLine("Error: {0}", ex.Message);
#endif
                }

                Console.WriteLine();
            }
        }
    }
}