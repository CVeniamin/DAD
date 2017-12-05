using OGP.Middleware;
using OGP.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;

namespace OGP.Client
{
    public partial class MainFrame : Form
    {
        // direction player is moving in. Only one will be true
        private bool goup;

        private bool godown;
        private bool goleft;
        private bool goright;

        private bool goupNew;
        private bool godownNew;
        private bool goleftNew;
        private bool gorightNew;

        private int boardRight = 320;
        private int boardBottom = 320;
        private int boardLeft = 0;
        private int boardTop = 40;

        //player speed
        private int speed = 5;

        private int score = 0; private int total_coins = 61;

        //ghost speed for the one direction ghosts
        private int ghost1 = 5;

        private int ghost2 = 5;

        //x and y directions for the bi-direccional pink ghost
        private int ghost3x = 5;

        private int ghost3y = 5;

        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        private delegate void SetTextDelegate(string text);

        private Thread chatThread = null;

        private void ThreadProcSafe(string text)
        {
            this.SetText(text);
        }

        // This method demonstrates a pattern for making thread-safe
        // calls on a Windows Forms control.
        //
        // If the calling thread is different from the thread that
        // created the TextBox control, this method creates a
        // StringArgReturningVoidDelegate and calls itself asynchronously using the
        // Invoke method.
        //
        // If the calling thread is the same as the thread that created
        // the TextBox control, the Text property is set directly.
        // usage
        //this.chatThread = new Thread(() => ThreadProcSafe(clientHostName));
        //this.chatThread.Start();

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.tbChat.InvokeRequired)
            {
                SetTextDelegate d = new SetTextDelegate(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.tbChat.Text += text;
            }
        }

        private IChatManager chatManager;

        private ChatClient chatClient;
        private PictureBox player;
        private PictureBox pacman;
        private PictureBox player2;

        private GameStateProxy gameState;
        private List<string> moves;

        //public MainFrame(ChatClient chat)
        internal MainFrame(ArgsOptions args)
        {
            InitializeComponent();

            Uri clientUri = new Uri(args.ClientUrl);
            string clientHostName = GetHostName(clientUri);

            List<Uri> serversURIs = new List<Uri>();
            foreach (var url in args.ServerEndpoints)
            {
                serversURIs.Add(new Uri(url));
            }

            string serverHostName = GetHostName(serversURIs[0]);

            TcpChannel channel = new TcpChannel(clientUri.Port);
            ChannelServices.RegisterChannel(channel, true);

            chatClient = new ChatClient(this, args.Pid, clientHostName);
            RemotingServices.Marshal(chatClient, "ChatClient");

            chatManager = (IChatManager)Activator.GetObject(typeof(IChatManager), serverHostName + "/ChatManager");
            chatManager.RegisterClient(clientHostName);

            Thread t = new Thread(() => WaitForClientsToStart(chatClient, chatManager));
            t.Start();

            moves = GetMoves(args.TraceFile);

            gameState = (GameStateProxy)Activator.GetObject(typeof(GameStateProxy), serverHostName + "/GameStateProxy");
            GameStateView gameView = gameState.GetGameState();
            this.pinkGhost = new PictureBox();
            this.yellowGhost = new PictureBox();
            this.redGhost = new PictureBox();

            foreach (GameGhost g in gameView.Ghosts)
            {
                switch (g.Type)
                {
                    case GhostType.Pink:
                        this.pinkGhost = DrawElement("pinkGhost", "ghost", global::OGP.Client.Properties.Resources.pink_guy, g.X, g.Y);
                        break;
                    case GhostType.Yellow:
                        this.yellowGhost = DrawElement("yellowGhost", "ghost", global::OGP.Client.Properties.Resources.yellow_guy, g.X, g.Y);
                        break;
                    case GhostType.Red:
                        this.redGhost = DrawElement( "redGhost", "ghost", global::OGP.Client.Properties.Resources.red_guy, g.X, g.Y);
                        break;
                }
                
            }

            //gamePlayer = new GamePlayer(11, 49, args.Pid, this.score, true);

            //DrawElement(player, "pacman", "pacman", global::OGP.Client.Properties.Resources.Left, gamePlayer.Location.X, gamePlayer.Location.Y);
            //RemotingServices.Marshal(gamePlayer, "GamePlayer");

            //gamePlayer = (GamePlayer)Activator.GetObject(typeof(GamePlayer), serverHostName + "/GamePlayer");


            //GamePlayer gp1 = new GamePlayer(11, 49, args.Pid, this.score, true);
            //gp1.DrawPlayer("pacman", global::OGP.Client.Properties.Resources.Left);

            this.pacman = DrawElement("pacman", "pacman", global::OGP.Client.Properties.Resources.Left, 11, 49);
            this.player = DrawElement("pacman", "pacman", global::OGP.Client.Properties.Resources.Left, 11, 90);

            //this.pinkGhost = DrawElement("pinkGhost", "ghost", global::OGP.Client.Properties.Resources.pink_guy, 200, 50);
            //this.yellowGhost = DrawElement("yellowGhost", "ghost", global::OGP.Client.Properties.Resources.yellow_guy, 200, 235);
            //this.redGhost = DrawElement("redGhost", "ghost", global::OGP.Client.Properties.Resources.red_guy, 240, 90);

            //GamePlayer gp2 = new GamePlayer(11, 90, args.Pid, this.score, true);
            //gp2.DrawPlayer("pacman", global::OGP.Client.Properties.Resources.Left);
            //this.player2 = gp2.GetPlayerImage();

            Thread move = new Thread(() => Play(this.player, args.TickDuration, args.TraceFile, moves));
            move.Start();

            label2.Visible = false;
        }

        public void Play(PictureBox image, int tick, string filename, List<string> moves)
        {
            int roundId = 0;
            int finalRound = moves.Count - 1;
            while (roundId != finalRound)
            {
                label1.Text = "Score: " + score;

                MovePacMan(image, moves[roundId]);

                if (goleftNew)
                {
                    if (image.Left > (boardLeft))
                        image.Left -= speed;
                }
                if (gorightNew)
                {
                    if (image.Left < (boardRight))
                        image.Left += speed;
                }
                if (goupNew)
                {
                    if (image.Top > (boardTop))
                        pacman.Top -= speed;
                }
                if (godownNew)
                {
                    if (image.Top < (boardBottom))
                        image.Top += speed;
                }
                //move ghosts
                redGhost.Left += ghost1;
                yellowGhost.Left += ghost2;

                // if the red ghost hits the wall 1 then wereverse the speed
                if (redGhost.Bounds.IntersectsWith(wallsArray[0].Bounds))
                    ghost1 = -ghost1;
                // if the red ghost hits the wall 2 we reverse the speed
                else if (redGhost.Bounds.IntersectsWith(wallsArray[1].Bounds))
                    ghost1 = -ghost1;
                // if the yellow ghost hits the wall 3 then we reverse the speed
                if (yellowGhost.Bounds.IntersectsWith(wallsArray[2].Bounds))
                    ghost2 = -ghost2;
                // if the yellow chost hits the wall 4 then wereverse the speed
                else if (yellowGhost.Bounds.IntersectsWith(wallsArray[3].Bounds))
                    ghost2 = -ghost2;
                //moving ghosts and bumping with the walls end
                //for loop to check walls, ghosts and points
                foreach (Control x in this.Controls)
                {
                    // checking if the player hits the wall or the ghost, then game is over
                    if (x is PictureBox && (string)x.Tag == "wall" || (string)x.Tag == "ghost")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(image.Bounds))
                        {
                            //image.Left = 0;
                            //image.Top = 25;
                            label2.Text = "GAME OVER";
                            label2.Visible = true;
                            //timer1.Stop();
                        }
                    }
                    if (x is PictureBox && (string)x.Tag == "coin")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(image.Bounds))
                        {
                            this.Controls.Remove(x);
                            score++;
                            //TODO check if all coins where "eaten"
                            if (score == total_coins)
                            {
                                //pacman.Left = 0;
                                //pacman.Top = 25;
                                label2.Text = "GAME WON!";
                                label2.Visible = true;
                                //timer1.Stop();
                            }
                        }
                    }
                }
                pinkGhost.Left += ghost3x;
                pinkGhost.Top += ghost3y;

                if (pinkGhost.Left < boardLeft ||
                    pinkGhost.Left > boardRight ||
                    (pinkGhost.Bounds.IntersectsWith(wallsArray[0].Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(wallsArray[1].Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(wallsArray[2].Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(wallsArray[3].Bounds)))
                {
                    ghost3x = -ghost3x;
                }
                if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
                {
                    ghost3y = -ghost3y;
                }
                Thread.Sleep(tick);
                roundId += 1;
            }
        }

        private List<string> GetMoves(string filename)
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

        public static string GetHostName(Uri uri)
        {
            return uri.ToString().Replace(uri.PathAndQuery, "");
        }

        public void WaitForClientsToStart(IChatClient chat, IChatManager manager)
        {
            while (!manager.GameStarted)
            {
                Thread.Sleep(1000);
            }

            if (chat != null && manager != null)
            {
                //each client receives a list containing all other clients
                chat.ClientsEndpoints = manager.GetClients();
                chat.ActivateClients();
            }
        }

        private void MovePacMan(PictureBox player, string move)
        {
            switch (move)
            {
                case "UP":
                    goupNew = true;
                    player.Image = Properties.Resources.Up;
                    break;
                case "DOWN":
                    godownNew = true;
                    player.Image = Properties.Resources.down;
                    break;
                case "LEFT":
                    goleftNew = true;
                    player.Image = Properties.Resources.Left;
                    break;
                case "RIGHT":
                    gorightNew = true;
                    player.Image = Properties.Resources.Right;
                    break;
            }
        }

        private void keyisdown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                //TODO: send to server moveLeft
                goleft = true;
                pacman.Image = Properties.Resources.Left;
            }
            if (e.KeyCode == Keys.Right)
            {
                //TODO: send to server moveRight
                goright = true;
                pacman.Image = Properties.Resources.Right;
            }
            if (e.KeyCode == Keys.Up)
            {
                //TODO: send to server moveUP
                goup = true;
                pacman.Image = Properties.Resources.Up;
            }
            if (e.KeyCode == Keys.Down)
            {
                //TODO: send to server moveDown
                godown = true;
                pacman.Image = Properties.Resources.down;
            }
            if (e.KeyCode == Keys.Enter)
            {
                tbMsg.Enabled = true; tbMsg.Focus();
            }
        }

        private void keyisup(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                goleft = false;
            }
            if (e.KeyCode == Keys.Right)
            {
                goright = false;
            }
            if (e.KeyCode == Keys.Up)
            {
                goup = false;
            }
            if (e.KeyCode == Keys.Down)
            {
                godown = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = "Score: " + score;
            //move player
            if (goleft)
            {
                if (pacman.Left > (boardLeft))
                    pacman.Left -= speed;
            }
            if (goright)
            {
                if (pacman.Left < (boardRight))
                    pacman.Left += speed;
            }
            if (goup)
            {
                if (pacman.Top > (boardTop))
                    pacman.Top -= speed;
            }
            if (godown)
            {
                if (pacman.Top < (boardBottom))
                    pacman.Top += speed;
            }
            //move ghosts
            redGhost.Left += ghost1;
            yellowGhost.Left += ghost2;

            // if the red ghost hits the wall 1 then wereverse the speed
            if (redGhost.Bounds.IntersectsWith(wallsArray[0].Bounds))
                ghost1 = -ghost1;
            // if the red ghost hits the wall 2 we reverse the speed
            else if (redGhost.Bounds.IntersectsWith(wallsArray[1].Bounds))
                ghost1 = -ghost1;
            // if the yellow ghost hits the wall 3 then we reverse the speed
            if (yellowGhost.Bounds.IntersectsWith(wallsArray[2].Bounds))
                ghost2 = -ghost2;
            // if the yellow chost hits the wall 4 then wereverse the speed
            else if (yellowGhost.Bounds.IntersectsWith(wallsArray[3].Bounds))
                ghost2 = -ghost2;
            //moving ghosts and bumping with the walls end
            //for loop to check walls, ghosts and points
            foreach (Control x in this.Controls)
            {
                // checking if the player hits the wall or the ghost, then game is over
                if (x is PictureBox && (string)x.Tag == "wall" || (string)x.Tag == "ghost")
                {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds))
                    {
                        pacman.Left = 0;
                        pacman.Top = 25;
                        label2.Text = "GAME OVER";
                        label2.Visible = true;
                        timer1.Stop();
                    }
                }
                if (x is PictureBox && (string)x.Tag == "coin")
                {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds))
                    {
                        this.Controls.Remove(x);
                        score++;
                        //TODO check if all coins where "eaten"
                        if (score == total_coins)
                        {
                            //pacman.Left = 0;
                            //pacman.Top = 25;
                            label2.Text = "GAME WON!";
                            label2.Visible = true;
                            timer1.Stop();
                        }
                    }
                }
            }
            pinkGhost.Left += ghost3x;
            pinkGhost.Top += ghost3y;

            if (pinkGhost.Left < boardLeft ||
                pinkGhost.Left > boardRight ||
                (pinkGhost.Bounds.IntersectsWith(wallsArray[0].Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(wallsArray[1].Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(wallsArray[2].Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(wallsArray[3].Bounds)))
            {
                ghost3x = -ghost3x;
            }
            if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
            {
                ghost3y = -ghost3y;
            }
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                chatClient.SendMsg(tbMsg.Text);
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        private void redGhost_Click(object sender, EventArgs e)
        {
        }

        private PictureBox redGhost;
        private PictureBox yellowGhost;
        private PictureBox pinkGhost;

        private PictureBox[] coinsArray = Enumerable.Repeat(0, 41).Select(c => new PictureBox()).ToArray();
        private PictureBox[] wallsArray = Enumerable.Repeat(0, 4).Select(w => new PictureBox()).ToArray();

        private void DrawPacman(PictureBox pacman, string pacmanName, Bitmap pacmanImage, int x, int y)
        {
            this.pacman.BackColor = Color.Transparent;
            this.pacman.Image = pacmanImage;
            this.pacman.Location = new Point(x, y);
            this.pacman.Margin = new Padding(0);
            this.pacman.Name = pacmanName;
            this.pacman.Size = new Size(33, 31);
            this.pacman.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pacman.TabIndex = 4;
            this.pacman.TabStop = false;

            ((ISupportInitialize)(pacman)).BeginInit();
        }
        private void DrawCoins()
        {
            short coinPosX = 15;
            short coinPosY = 45;
            short tabIndex = 73;
            short columnsX = 1;
            for (int i = 0; i < coinsArray.Length; i++)
            {
                //completed one row, reset x and y positions to new row
                if (columnsX == 7)
                {
                    coinPosX = 15;
                    coinPosY += 45;
                    columnsX = 1;
                }

                coinsArray[i].Image = global::OGP.Client.Properties.Resources.cccc;
                coinsArray[i].Location = new Point(coinPosX, coinPosY);
                coinsArray[i].Margin = new Padding(4);
                coinsArray[i].Name = "pictureBox" + i.ToString();
                coinsArray[i].Size = new Size(15, 15);
                coinsArray[i].SizeMode = PictureBoxSizeMode.StretchImage;
                coinsArray[i].TabIndex = tabIndex;
                coinsArray[i].TabStop = false;
                coinsArray[i].Tag = "coin";

                coinPosX += 60;
                columnsX++;
                tabIndex++;

                ((ISupportInitialize)(coinsArray[i])).BeginInit();
            }
        }

        /// <summary>
        /// Method used to draw a ghost give a Object, name, resource and x,y
        /// </summary>
        ///
        private PictureBox DrawElement(string ghostname, string tag,  Bitmap resource, int x, int y)
        {

            PictureBox element = new PictureBox();
            element.BackColor = Color.Transparent;
            element.Image = resource;
            element.Location = new Point(x, y);
            element.Margin = new Padding(4);
            element.Name = ghostname;
            element.Size = new Size(35, 35);
            element.SizeMode = PictureBoxSizeMode.Zoom;
            element.TabIndex = 3;
            element.TabStop = false;
            element.Tag = tag;
            InitializeDrawing(element);
            return element;
        }

        public void InitializeDrawing(PictureBox element)
        {
            ((ISupportInitialize)(element)).BeginInit();
            this.Controls.Add(element);
            ((ISupportInitialize)(element)).EndInit();
        }

        /// <summary>
        /// Method used to draw a wall give a Object, name, pos(x,y) and size(w,h)
        /// </summary>
        private void DrawWall(PictureBox wall, string name, int[] pos)
        {
            wall.BackColor = Color.MidnightBlue;
            wall.Location = new Point(pos[0], pos[1]);
            wall.Margin = new Padding(4);
            wall.Name = name;
            wall.Size = new Size(15, 80);
            wall.SizeMode = PictureBoxSizeMode.Zoom;
            wall.TabIndex = 3;
            wall.TabStop = false;
            wall.Tag = "wall";

            ((ISupportInitialize)(wall)).BeginInit();
        }

        private void MainFrame_Load(object sender, EventArgs e)
        {

            //this.pinkGhost = new PictureBox();
            //this.yellowGhost = new PictureBox();
            //this.redGhost = new PictureBox();

            int[] wall1 = new int[] { 110, 50 };
            int[] wall2 = new int[] { 280, 50 };
            int[] wall3 = new int[] { 110, 245 };
            int[] wall4 = new int[] { 300, 245 };

            int[][] walls = { wall1, wall2, wall3, wall4 };

            for (int i = 0; i < wallsArray.Length; i++)
            {
                DrawWall(wallsArray[i], "wall" + i.ToString(), walls[i]);
            }

            DrawCoins();

            void addControlFor(PictureBox[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    this.Controls.Add(array[i]);
                }
            }

            addControlFor(wallsArray);
            addControlFor(coinsArray);

            void EndInitFor(PictureBox[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    ((ISupportInitialize)(array[i])).EndInit();
                }
            }

            EndInitFor(wallsArray);
            EndInitFor(coinsArray);
        }

        public void AddMsg(string s)
        {
            tbChat.Text += "\r\n" + s;
        }

        private void TbChat_MouseDown(object sender, MouseEventArgs e)
        {
            if (!tbChat.Focused)
                tbChat.Focus();
        }

        private void tbChat_TextChanged(object sender, EventArgs e)
        {
            this.tbChat.SelectionStart = this.tbChat.Text.Length;
            this.tbChat.ScrollToCaret();
        }
    }
}