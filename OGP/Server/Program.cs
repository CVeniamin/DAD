using Sprache;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
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

            if (argsOptions.Pcs != true)
            {
                Console.WriteLine("Started Server with PID: " + argsOptions.Pid);
            }

            // Create and register a remoting channel
            try
            {
                TcpChannel channel = new TcpChannel(new Uri(argsOptions.ServerUrl).Port);
                ChannelServices.RegisterChannel(channel, true);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Could not bind to port. Either already occupied or blocked by firewall. Exiting.");
                throw new Exception("Socket not available");
            }

            // Create GameState object - used to store the current local state of the system
            GameState gameState = new GameState(existsingServersList);

            // Create OutManager - for sending messages out
            OutManager outManager = new OutManager(argsOptions.ServerUrl, existsingServersList.Count > 0 ? existsingServersList[0] : null, gameState);

            // Create action handler - for processing movements (when master)
            ActionHandler actionHandler = new ActionHandler(gameState, argsOptions.NumPlayers);
            actionHandler.SetOutManager(outManager);

            // Create state handler - for processing state updates (when slave)
            StateHandler stateHandler = new StateHandler((GameStateView gameStateView) =>
            {
                gameState.Patch(gameStateView);
            });
            stateHandler.SetOutManager(outManager);

            // Create InManager - Remoting endpoint is made available here
            InManager inManager = new InManager(argsOptions.ServerUrl, actionHandler, null, stateHandler);

            // Begin server Timer
            new Thread(() =>
            {
                long tickId = 0;

                while (true)
                {
                    if (outManager.GetMasterServer() != argsOptions.ServerUrl)
                    {
                        // This is a slave
                        outManager.SendCommand(new Command
                        {
                            Type = CommandType.Action
                        }, OutManager.MASTER_SERVER);
                    }
                    else
                    {
                        // This is a master
                        actionHandler.FinalizeTick(tickId);
                    }

                    try
                    {
                        Thread.Sleep(argsOptions.TickDuration);
                    }
                    catch (ThreadInterruptedException) { }

                    tickId++;
                }
            }).Start();

            // Start listening for input
            while (true)
            {
                var input = Console.ReadLine();

                if (input == null || input.Trim() == "Quit")
                {
                    Console.WriteLine("Exit triggered by input");
                    break;
                }

                try
                {
                    ICommand cmd = CommandParser.Command.Parse(input);

                    string result = cmd.Exec(gameState, inManager, outManager);
                    if (result != String.Empty)
                    {
                        Console.Error.WriteLine(result);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}