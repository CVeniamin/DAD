using OGP.Middleware;
using OGP.Server;
using Sprache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;

namespace OGP.Client
{
    internal class Program
    {
        public static int roundId = -1;
        public static bool gameOver = false;

        delegate void PrintChatMessage(string x);
        delegate void IngestGameStateView(GameStateView gameStateView);

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

            if (argsOptions.Pcs == true)
            {
                Console.WriteLine("Suppressing output");
                // Console.SetOut(new SuppressedWriter());
            }

            Console.WriteLine("Started Client with PID: " + argsOptions.Pid);

            // Create and register a remoting channel
            try
            {
                TcpChannel channel = new TcpChannel(new Uri(argsOptions.ClientUrl).Port);
                ChannelServices.RegisterChannel(channel, true);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.SocketErrorCode + " " + ex.NativeErrorCode + " " + ex.Message + " " + ex.HelpLink);
                Console.WriteLine("Could not bind to port. Either already occupied or blocked by firewall. Exiting.", "CRITICAL"); // TODO: Remove?
                throw new Exception("Socket not available");
            }

            // Create GameState object - used to store the current local state of the system
            GameState gameState = new GameState(existsingServersList);
            
            // Create OutManager - for sending messages out
            OutManager outManager = new OutManager(argsOptions.ClientUrl, existsingServersList, gameState);

            MainFrame mainForm = new MainFrame(argsOptions.Pid, outManager);

            // Create chat handler - for processing chat messages
            ChatHandler chatHandler = new ChatHandler((ChatMessage chatMessage) =>
            {
                Console.WriteLine("Received chat message from {0}: {1}", chatMessage.Sender, chatMessage.Message);
                
                mainForm.Invoke(new PrintChatMessage(mainForm.AppendMessageToChat), chatMessage.Message);
            });
            chatHandler.SetOutManager(outManager);

            // Create state handler - for processing state updates (when slave)
            StateHandler stateHandler = new StateHandler((GameStateView gameStateView) =>
            {
                mainForm.Invoke(new IngestGameStateView(mainForm.ApplyGameStateView), gameStateView);

                roundId = gameStateView.RoundId;
                gameOver = gameStateView.GameOver;
                //save state to memory
            });
            stateHandler.SetOutManager(outManager);
            
            // Create InManager - Remoting endpoint is made available here
            InManager inManager = new InManager(argsOptions.ClientUrl, null, chatHandler, stateHandler);

            // Begin client Timer
            new Thread(() => ActionDispatcher(outManager, replayMoves, argsOptions)).Start();
            
            Application.Run(mainForm);
            
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

        private static void ActionDispatcher(OutManager outManager, Dictionary<int, Direction> replayMoves, ArgsOptions argsOptions)
        {
            {
                int tickId = 0;
                bool replayingMoves = replayMoves.Count > 0;
                bool sentThisTick = false;

                //create new player on server 
                outManager.SendCommand(new Command
                {
                    Type = Server.CommandType.Action,
                    Args = new ClientSync
                    {
                        Pid = argsOptions.Pid
                    }
                }, OutManager.MASTER_SERVER);

                while (true)
                {
                    sentThisTick = false;

                    Console.WriteLine("round Id at " + roundId);
                    
                    if (replayingMoves && replayMoves.TryGetValue(roundId, out Direction nextMove) && !gameOver)
                    {
                        
                        outManager.SendCommand(new Command
                        {
                            Type = Server.CommandType.Action,
                            Args = new GameMovement
                            {
                                Direction = nextMove
                            }
                        }, OutManager.MASTER_SERVER);
                            
                        sentThisTick = true;
                    }
                    else
                    {
                        // Get moves from keyboard, or send idle events regardless (only send moves when there is a direction change)
                    }

                    // TODO: send request for state to server, in case no keyboard input was made
                    if (!sentThisTick)
                    {
                        outManager.SendCommand(new Command
                        {
                            Type = Server.CommandType.Action,
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