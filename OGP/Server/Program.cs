using Sprache;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace OGP.Server
{
    internal class Program
    {   
        private static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();

            if (!CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Console.WriteLine("Missing required arguments");
                return;
            }

            List<string> existsingServersList = argsOptions.ServerEndpoints != null ? (List<string>)argsOptions.ServerEndpoints : new List<string>();
            existsingServersList.Add(argsOptions.ServerUrl);

            if (argsOptions.Pcs == true)
            {
                Console.WriteLine("Suppressing output");
                //Console.SetOut(new SuppressedWriter());
            }

            Console.WriteLine("Started Server with PID: " + argsOptions.Pid);
            
            // Create and register a remoting channel
            TcpChannel channel = new TcpChannel(new Uri(argsOptions.ServerUrl).Port);
            ChannelServices.RegisterChannel(channel, true);

            // Create GameState object - used to store the current local state of the system
            GameState gameState = new GameState(existsingServersList);

            // Create OutManager - for sending messages out
            OutManager outManager = new OutManager(argsOptions.ServerUrl, existsingServersList, gameState);
            
            // Create action handler - for processing movements (when master)
            ActionHandler actionHandler = new ActionHandler(gameState, argsOptions.NumPlayers);
            actionHandler.SetOutManager(outManager);

            // Create state handler - for processing state updates (when slave)
            StateHandler stateHandler = new StateHandler(gameState);
            stateHandler.SetOutManager(outManager);

            // Create InManager - Remoting endpoint is made available here
            InManager inManager = new InManager(argsOptions.ServerUrl, actionHandler, null, stateHandler, true);

            // Begin server Timer
            new Thread(() => {
                long tickId = 0;

                while (true)
                {
                    actionHandler.FinilizeTick(tickId);

                    try
                    {
                        Thread.Sleep(argsOptions.TickDuration);
                    }
                    catch (ThreadInterruptedException) { }
                }
            }).Start();
                
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
                cmd.Exec(gameState);
            }
        }
    }
}