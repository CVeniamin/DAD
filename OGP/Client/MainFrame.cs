using OGP.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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

        public ChatClient chatClient;

        public MainFrame(string[] args)
        {
            InitializeComponent();

            //only works without filename provided
            string clientURL = args[5];
            Uri clientUri = new Uri(clientURL);
            string clientHostName = clientUri.ToString().Replace(clientUri.PathAndQuery, "");

            string serverURL = args[11];
            string[] serverURLS = serverURL.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> namesList = new List<string>(serverURLS.Length);
            namesList.AddRange(serverURLS);
            List<Uri> serversURIs = new List<Uri>();
            foreach (var n in namesList)
            {
                serversURIs.Add(new Uri(n));
            }

            string serverHostName = serversURIs[0].ToString().Replace(serversURIs[0].PathAndQuery, "");

            this.chatThread = new Thread(() => ThreadProcSafe(clientHostName));
            this.chatThread.Start();

            TcpChannel channel = new TcpChannel(clientUri.Port);
            ChannelServices.RegisterChannel(channel, true);

            string pid = args[1];
            chatClient = new ChatClient(this, pid);

            this.chatThread = new Thread(() => ThreadProcSafe(chatClient.Pid));
            this.chatThread.Start();

            RemotingServices.Marshal(chatClient, "ChatClient");

            chatManager = (IChatManager)Activator.GetObject(typeof(IChatManager), serverHostName + "/ChatManager");

            chatManager.RegisterClient(clientHostName);
            chatClient.Clients = chatManager.getClients();

            label2.Visible = false;
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
                int i = 1;
                foreach (var v in chatClient.Clients)
                {
                    tbChat.Text += i.ToString();
                    i++;
                }

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
                tbChat.Text += "\r\n" + tbMsg.Text; tbMsg.Clear(); tbMsg.Enabled = false; this.Focus();
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

                ((System.ComponentModel.ISupportInitialize)(coinsArray[i])).BeginInit();
            }
        }

        /// <summary>
        /// Method used to draw a ghost give a Object, name, resource and x,y
        /// </summary>
        ///
        private void DrawGhost(PictureBox ghost, string ghostname, Bitmap ghostResource, int x, int y)
        {
            ghost.BackColor = Color.Transparent;
            ghost.Image = ghostResource;
            ghost.Location = new Point(x, y);
            ghost.Margin = new Padding(4);
            ghost.Name = ghostname;
            ghost.Size = new Size(40, 37);
            ghost.SizeMode = PictureBoxSizeMode.Zoom;
            ghost.TabIndex = 3;
            ghost.TabStop = false;
            ghost.Tag = "ghost";

            ((System.ComponentModel.ISupportInitialize)(ghost)).BeginInit();
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

            ((System.ComponentModel.ISupportInitialize)(wall)).BeginInit();
        }

        private static Game GetGameObject(String url, int port, String objName)
        {
            TcpChannel chan = new TcpChannel();
            ChannelServices.RegisterChannel(chan, true);
            String endpoint = url + port.ToString() + "/" + objName;
            Game game = (Game)Activator.GetObject(
              typeof(Game), endpoint);
            return game;
        }

        private void MainFrame_Load(object sender, EventArgs e)
        {
            //Game game = GetGameObject("tcp://localhost:", 8086, "GameObject");
            //string hello = game.SayHello();
            //tbChat.Text += hello;
            this.pinkGhost = new PictureBox();
            this.yellowGhost = new PictureBox();
            this.redGhost = new PictureBox();

            int[] wall1 = new int[] { 110, 50 };
            int[] wall2 = new int[] { 280, 50 };
            int[] wall3 = new int[] { 110, 245 };
            int[] wall4 = new int[] { 300, 245 };

            int[][] walls = { wall1, wall2, wall3, wall4 };

            for (int i = 0; i < wallsArray.Length; i++)
            {
                DrawWall(wallsArray[i], "wall" + i.ToString(), walls[i]);
            }

            DrawGhost(this.pinkGhost, "pinkGhost", global::OGP.Client.Properties.Resources.pink_guy, 200, 50);
            DrawGhost(this.yellowGhost, "yellowGhost", global::OGP.Client.Properties.Resources.yellow_guy, 200, 235);
            DrawGhost(this.redGhost, "redGhost", global::OGP.Client.Properties.Resources.red_guy, 240, 90);

            this.Controls.Add(this.pinkGhost);
            this.Controls.Add(this.yellowGhost);
            this.Controls.Add(this.redGhost);

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

            ((System.ComponentModel.ISupportInitialize)(this.pinkGhost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yellowGhost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.redGhost)).EndInit();

            void EndInitFor(PictureBox[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    ((System.ComponentModel.ISupportInitialize)(array[i])).EndInit();
                }
            }

            EndInitFor(wallsArray);
            EndInitFor(coinsArray);
        }

        public void AddMsg(string s)
        {
            tbChat.Text += "\r\n" + s;
        }

        private delegate void DelAddMsg(string mensagem);

        public class ChatClient : MarshalByRefObject, IChatClient
        {
            public static MainFrame form;

            public void MsgToClient(string mensagem)
            {
                // thread-safe access to form
                form.Invoke(new DelAddMsg(form.AddMsg), mensagem);
            }

            private List<IChatClient> clients;
            private List<string> messages;
            private string pid;

            public List<IChatClient> Clients { get => clients; set => clients = value; }
            public string Pid { get => pid; set => pid = value; }

            public ChatClient(MainFrame mf, string p )
            {
                Clients = new List<IChatClient>();
                messages = new List<string>();
                form = mf;
                pid = p;
            }

            public void SendMsg(string mensagem)
            {
                messages.Add(mensagem);
                ThreadStart ts = new ThreadStart(this.BroadcastMessage);
                Thread t = new Thread(ts);
                t.Start();
            }

            private void BroadcastMessage()
            {
                string MsgToBcast;
                lock (this)
                {
                    MsgToBcast = messages[messages.Count - 1];
                }
                for (int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        //ChatClient cc = (ChatClient) clients[i];
                        //MsgToClient(cc.pid);
                        //cc.MsgToClient(MsgToBcast);
                        ((IChatClient)clients[i]).MsgToClient(MsgToBcast);
                    }
                    catch (Exception e)
                    {

                        MsgToClient(e.ToString());
                        clients.RemoveAt(i);
                    }
                }
            }
        }
    }
}