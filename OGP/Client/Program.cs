using OGP.Middleware;
using OGP.Server;
using Sprache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;

namespace OGP.Client
{
    internal class Program
    {
        delegate void PrintChatMessage(string x);

        [STAThread]
        public static void Main(string[] args)
        {
            var argsOptions = new ArgsOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, argsOptions))
            {
                Console.WriteLine("Missing required arguments");
                return;
            }

            List<string> existsingServersList = argsOptions.ServerEndpoints != null ? (List<string>)argsOptions.ServerEndpoints : new List<string>();
            List<string> replayMoves = argsOptions.TraceFile != null ? LoadMoves(argsOptions.TraceFile) : new List<string>();

            if (argsOptions.Pcs == true)
            {
                Console.WriteLine("Suppressing output");
                // Console.SetOut(new SuppressedWriter());
            }

            Console.WriteLine("Started Client with PID: " + argsOptions.Pid);

            // Create and register a remoting channel
            TcpChannel channel = new TcpChannel(new Uri(argsOptions.ClientUrl).Port);
            ChannelServices.RegisterChannel(channel, true);

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
            StateHandler stateHandler = new StateHandler(gameState);
            stateHandler.SetOutManager(outManager);
            
            // Create InManager - Remoting endpoint is made available here
            InManager inManager = new InManager(argsOptions.ClientUrl, null, chatHandler, stateHandler);

            // Begin client Timer
            new Thread(() => {
                int tickId = 0;
                bool replayingMoves = replayMoves.Count > 0;

                while (true)
                {
                    if (replayingMoves)
                    {
                        string nextMove = replayMoves.ElementAtOrDefault(tickId);
                        if (nextMove != null)
                        {
                            // TODO: send nextMove
                        }
                        else
                        {
                            replayingMoves = false;
                        }
                    }
                    else
                    {
                        // Get moves from keyboard, or send idle events regardless (only send moves when there is a direction change)
                    }
                    
                    // TODO: send request for state to server, in case no keyboard input was made

                    try
                    {
                        Thread.Sleep(argsOptions.TickDuration);
                    }
                    catch (ThreadInterruptedException) { }

                    tickId++;
                }
            }).Start();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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

        private static List<string> LoadMoves(string filename)
        {
            List<string> moves = new List<string>();

            if (filename != null && filename != String.Empty)
            {
                using (var reader = new StreamReader(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        moves.Add(values[1]);
                    }
                }
            }

            return moves;
        }
    }
}