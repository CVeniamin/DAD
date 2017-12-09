using OGP.Server;
using Sprache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using OGP.Middleware;
using System.Linq;

namespace OGP.Client
{
    internal class Program
    {
        private delegate void PrintChatMessage(string msg);

        private delegate void IngestGameStateView(GameStateView gameStateView);
        [STAThread]
        public static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Console.WriteLine("Missing required arguments");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<string> existsingServersList = argsOptions.ServerEndpoints != null ? (List<string>)argsOptions.ServerEndpoints : new List<string>();
            Dictionary<int, Direction> replayMoves = argsOptions.TraceFile != null ? LoadMoves(argsOptions.TraceFile) : new Dictionary<int, Direction>();

            if (argsOptions.Pcs != true)
            {
                Console.WriteLine("Started Client with PID: " + argsOptions.Pid);
            }
            
            // Create and register a remoting channel
            try
            {
                TcpChannel channel = new TcpChannel(new Uri(argsOptions.ClientUrl).Port);
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
            OutManager outManager = new OutManager(argsOptions.ClientUrl, existsingServersList.Count > 0 ? existsingServersList[0] : null, gameState);

            MainFrame mainForm = new MainFrame(argsOptions, outManager);
            IngestGameStateView gameStateViewIngest = new IngestGameStateView(mainForm.ApplyGameStateView);

            // Create chat handler - for processing chat messages
            ChatHandler chatHandler = new ChatHandler((ChatMessage chatMessage) =>
            {
                try
                {
                    mainForm.Invoke(new PrintChatMessage(mainForm.AppendMessageToChat), String.Format("{0} : {1} ", chatMessage.Sender, chatMessage.Message));
                }
                catch (ThreadInterruptedException)
                {
                }
            });
            chatHandler.SetOutManager(outManager);

            // Create state handler - for processing state updates (when slave)
            StateHandler stateHandler = new StateHandler((GameStateView gameStateView) =>
            {
                try
                {
                    gameState.Patch(gameStateView);
                }
                catch (Exception)
                {
                }

                try
                {
                    mainForm.Invoke(gameStateViewIngest, gameStateView);
                }
                catch (Exception)
                {
                }
            });
            stateHandler.SetOutManager(outManager);

            // Create InManager - Remoting endpoint is made available here
            InManager inManager = new InManager(argsOptions.ClientUrl, null, chatHandler, stateHandler);

            // Begin client Timer
            new Thread(() => ActionDispatcher(outManager, replayMoves, argsOptions, gameState, mainForm)).Start();

            if (replayMoves.Count > 0)
            {
                Console.WriteLine("Replaying moves from trace file. Keyboard events ignored.");
                mainForm.IgnoreKeyboard = true;
            }

            new Thread(() => Application.Run(mainForm)).Start();

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
        
        private static void ActionDispatcher(OutManager outManager, Dictionary<int, Direction> replayMoves, ArgsOptions argsOptions,
            GameState gameState, MainFrame mainForm)
        {
            {
                int tickId = 0;
                bool sentThisTick = false;

                //create new player on server
                outManager.SendCommand(new Command
                {
                    Type = CommandType.Action,
                    Args = new ClientSync
                    {
                        Pid = argsOptions.Pid
                    }
                }, OutManager.MASTER_SERVER);

                int totalMoves = replayMoves.Count;
                int replayedMoves = 0;
                int maxRoundId = replayMoves.Keys.ToArray().Max();
                bool replayingMoves = totalMoves > 0;

                while (true)
                {
                    sentThisTick = false;
                    
                    if (replayingMoves && replayMoves.TryGetValue(gameState.RoundId, out Direction nextMove) && !gameState.GameOver)
                    {
                        replayedMoves++;

                        outManager.SendCommand(new Command
                        {
                            Type = CommandType.Action,
                            Args = new GameMovement
                            {
                                Direction = nextMove
                            }
                        }, OutManager.MASTER_SERVER);

                        sentThisTick = true;
                        
                        if (replayedMoves >= totalMoves - 1 || gameState.RoundId >= maxRoundId)
                        {
                            replayingMoves = false;

                            Console.WriteLine("Trace file eneded. Keyboard events enabled.");
                            mainForm.IgnoreKeyboard = false;
                        }
                    }
                    
                    if (!sentThisTick)
                    {
                        outManager.SendCommand(new Command
                        {
                            Type = CommandType.Action,
                            Args = new ClientSync
                            {
                                Pid = argsOptions.Pid
                            }
                        }, OutManager.MASTER_SERVER);
                    }

                    try
                    {
                        Thread.Sleep(argsOptions.TickDuration);
                    }
                    catch (ThreadInterruptedException) { }

                    tickId++;
                }
            }
        }

        private static Dictionary<int, Direction> LoadMoves(string filename)
        {
            Dictionary<int, Direction> moves = new Dictionary<int, Direction>();

            if (filename != null && filename != String.Empty)
            {
                using (var reader = new StreamReader(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        var values = reader.ReadLine().Split(',');

                        Direction direction = (Direction)Enum.Parse(typeof(Direction), values[1], true);
                        if (Enum.IsDefined(typeof(Direction), direction))
                        {
                            moves.Add(Int32.Parse(values[0]), direction);
                        }
                    }
                }
            }

            return moves;
        }
    }
}