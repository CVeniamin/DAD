using OGP.Middleware;
using OGP.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace OGP.Client
{
    public partial class MainFrame : Form
    {
        private string Pid;
        private int numPlayers;
        private OutManager outManager;

        private Direction lastSentDirection;

        private Size coinSize;
        private Size ghostSize;
        private Size playerSize;
        
        private PictureBox[] coinPictureBoxes;
        private PictureBox[] ghostsArray;
        private PictureBox[] wallPictureBoxes;
        private Dictionary<string, PictureBox> playerPictureBoxes;
        private bool pictureBoxesReady = false;


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
        private List<string> moves;

        private PictureBox player;
        private PictureBox pacman;
        private PictureBox redGhost;
        private PictureBox yellowGhost;
        private PictureBox pinkGhost;
        
        private Object lockRoundId = new Object();


        internal MainFrame(ArgsOptions argsOptions, OutManager outManager)
        {
            InitializeComponent();
            PrepareCommonUIObjects();

            this.Pid = argsOptions.Pid;
            this.numPlayers = argsOptions.NumPlayers;
            this.outManager = outManager;
            
            label2.Visible = true;
            label2.Text = "Waiting for players...";
        }

        private void PrepareCommonUIObjects()
        {
            coinSize = new Size(ObjectDimensions.COIN_WIDTH, ObjectDimensions.COIN_HEIGHT);
            ghostSize = new Size(ObjectDimensions.GHOST_WIDTH, ObjectDimensions.GHOST_HEIGHT);
            playerSize = new Size(ObjectDimensions.PLAYER_WIDTH, ObjectDimensions.PLAYER_HEIGHT);
        }
        
        private void LoadPictureBoxes(GameStateView gameStateView)
        {
            wallPictureBoxes = Enumerable.Repeat(0, gameStateView.Walls.Count).Select(c => new PictureBox
            {
                BackColor = Color.MidnightBlue,
                Margin = new Padding(4),
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            }).ToArray();

            coinPictureBoxes = Enumerable.Repeat(0, gameStateView.Coins.Count).Select(c => new PictureBox
            {
                Image = Properties.Resources.cccc,
                Margin = new Padding(4),
                Size = coinSize,
                SizeMode = PictureBoxSizeMode.StretchImage,
                TabStop = false
            }).ToArray();

            pinkGhost = CreateGhostPictureBox(Properties.Resources.pink_guy);
            yellowGhost = CreateGhostPictureBox(Properties.Resources.yellow_guy);
            redGhost = CreateGhostPictureBox(Properties.Resources.red_guy);
            
            playerPictureBoxes = new Dictionary<string, PictureBox>();

            DrawWalls(gameStateView.Walls); // Draw once
            DrawCoins(gameStateView.Coins); // Draw once
            DrawGhosts(); // Draw once
            
            pictureBoxesReady = true;
        }

        private PictureBox CreateGhostPictureBox(Bitmap resource)
        {
            return new PictureBox
            {
                BackColor = Color.Transparent,
                Image = resource,
                Margin = new Padding(4),
                Size = ghostSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            };
        }

        private PictureBox CreatePlayerPictureBox()
        {
            return new PictureBox
            {
                BackColor = Color.Transparent,
                Margin = new Padding(4),
                Size = playerSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false
            };
        }

        public void InitializeDrawing(PictureBox element)
        {
            ((ISupportInitialize)(element)).BeginInit();
            this.Controls.Add(element);
            ((ISupportInitialize)(element)).EndInit();
        }

        private void DrawWalls(List<Wall> Walls)
        {
            int i = 0;
            foreach (Wall wall in Walls)
            {
                wallPictureBoxes[i].Location = new Point(wall.X, wall.Y);
                wallPictureBoxes[i].Size = new Size(wall.Width, wall.Height);
                InitializeDrawing(wallPictureBoxes[i]);
                i++;
            }
        }

        private void DrawCoins(List<Coin> Coins)
        {
            int i = 0;
            foreach (Coin coin in Coins)
            {
                coinPictureBoxes[i].Location = new Point(coin.X, coin.Y);
                InitializeDrawing(coinPictureBoxes[i]);
                i++;
            }
        }

        private void DrawGhosts()
        {
            InitializeDrawing(pinkGhost);
            InitializeDrawing(yellowGhost);
            InitializeDrawing(redGhost);
        }

        internal void ApplyGameStateView(GameStateView gameStateView)
        {
            if (!pictureBoxesReady)
            {
                LoadPictureBoxes(gameStateView);
            }
            
            UpdateCoins(gameStateView);
            UpdateGhosts(gameStateView);
            DisplayPlayers(gameStateView);
        }
        
        private PictureBox DrawElement(string ghostname, string tag, Bitmap resource, int x, int y, Size size)
        {
            PictureBox element = new PictureBox
            {
                BackColor = Color.Transparent,
                Image = resource,
                Location = new Point(x, y),
                Margin = new Padding(4),
                Name = ghostname,
                Size = size,
                SizeMode = PictureBoxSizeMode.Zoom,
                TabStop = false,
                Tag = tag
            };
            InitializeDrawing(element);
            return element;
        }

        private void UpdateCoins(GameStateView gameStateView)
        {
            int i = 0;
            foreach (Coin coin in gameStateView.Coins)
            {
                coinPictureBoxes[i].Visible = coin.Visible;
                i++;
            }
        }

        private void UpdateGhosts(GameStateView gameStateView)
        {
            foreach (Ghost ghost in gameStateView.Ghosts)
            {
                switch (ghost.Color)
                {
                    case GhostColor.Pink:
                        pinkGhost.Location = new Point(ghost.X, ghost.Y);
                        break;
                    case GhostColor.Yellow:
                        yellowGhost.Location = new Point(ghost.X, ghost.Y);
                        break;
                    case GhostColor.Red:
                        redGhost.Location = new Point(ghost.X, ghost.Y);
                        break;
                }
            }
        }

        private void DisplayPlayers(GameStateView gameStateView)
        {
            foreach (Player player in gameStateView.Players)
            {
                if (!playerPictureBoxes.TryGetValue(player.PlayerId, out PictureBox pictureBox))
                {
                    pictureBox = CreatePlayerPictureBox();
                    InitializeDrawing(pictureBox);
                    playerPictureBoxes.Add(player.PlayerId, pictureBox);
                }
                
                if (player.Alive)
                {
                    Bitmap updatedBitmap = DirectionToResource(player.Direction);
                    if (updatedBitmap != null && pictureBox.Image != updatedBitmap)
                    {
                        pictureBox.Image = updatedBitmap;
                    }

                    pictureBox.Location = new Point(player.X, player.Y);
                } else
                {
                    pictureBox.Visible = false;
                }
            }
        }

        private Bitmap DirectionToResource(Direction direction)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    return Properties.Resources.Left;

                case Direction.RIGHT:
                    return Properties.Resources.Right;

                case Direction.UP:
                    return Properties.Resources.Up;

                case Direction.DOWN:
                    return Properties.Resources.down;
            }

            return null;
        }
        


        private void Play()
        {
            PictureBox player = null;

            int tick = 20;
            List<string> moves = new List<string>();
            int roundId = 0;

            while (roundId < moves.Count)
            {

                label2.SetPropertyThreadSafe(() => label2.Text, moves[roundId]);
                label1.Text = "Score: " + score;

                MovePacMan(player, moves[roundId]);

                //coinsArray[1].SetPropertyThreadSafe(() => coinsArray[1].Left, coinsArray[1].Left = coinsArray[1].Left + speed);
                //coinsArray[3].SetPropertyThreadSafe(() => coinsArray[1].Left, coinsArray[1].Left + speed);

                if (goleftNew)
                {
                    if (player.Left > (boardLeft))
                        player.Left -= speed;
                }
                if (gorightNew)
                {
                    if (player.Left < (boardRight))
                        player.Left += speed;
                }
                if (goupNew)
                {
                    if (player.Top > (boardTop))
                        player.Top -= speed;
                }
                if (godownNew)
                {
                    if (player.Top < (boardBottom))
                        player.Top += speed;
                }
                //move ghosts
                redGhost.Left += ghost1;
                yellowGhost.Left += ghost2;

                // if the red ghost hits the wall 1 then wereverse the speed
                if (redGhost.Bounds.IntersectsWith(wallPictureBoxes[0].Bounds))
                    ghost1 = -ghost1;
                // if the red ghost hits the wall 2 we reverse the speed
                else if (redGhost.Bounds.IntersectsWith(wallPictureBoxes[1].Bounds))
                    ghost1 = -ghost1;
                // if the yellow ghost hits the wall 3 then we reverse the speed
                if (yellowGhost.Bounds.IntersectsWith(wallPictureBoxes[2].Bounds))
                    ghost2 = -ghost2;
                // if the yellow chost hits the wall 4 then wereverse the speed
                else if (yellowGhost.Bounds.IntersectsWith(wallPictureBoxes[3].Bounds))
                    ghost2 = -ghost2;
                //moving ghosts and bumping with the walls end
                //for loop to check walls, ghosts and points
                foreach (Control x in this.Controls)
                {
                    // checking if the player hits the wall or the ghost, then game is over
                    if (x is PictureBox && (string)x.Tag == "wall" || (string)x.Tag == "ghost")
                    {
                        if (((PictureBox)x).Bounds.IntersectsWith(player.Bounds))
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
                        if (((PictureBox)x).Bounds.IntersectsWith(player.Bounds))
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
                    (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[0].Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[1].Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[2].Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[3].Bounds)))
                {
                    ghost3x = -ghost3x;
                }
                if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
                {
                    ghost3y = -ghost3y;
                }

                Thread.Sleep(tick);

                lock (lockRoundId)
                {
                    roundId++;
                }
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

        private void SendDirection(Direction direction)
        {
            if (direction != lastSentDirection)
            {
                outManager.SendCommand(new Command
                {
                    Type = Server.CommandType.Action,
                    Args = new GameMovement
                    {
                        Direction = direction
                    }
                }, OutManager.MASTER_SERVER);
            }
        }

        private void Keyisdown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                SendDirection(Direction.LEFT);
                goleft = true;
                pacman.Image = Properties.Resources.Left;
            }
            if (e.KeyCode == Keys.Right)
            {
                SendDirection(Direction.RIGHT);
                goright = true;
                pacman.Image = Properties.Resources.Right;
            }
            if (e.KeyCode == Keys.Up)
            {
                SendDirection(Direction.UP);
                goup = true;
                pacman.Image = Properties.Resources.Up;
            }
            if (e.KeyCode == Keys.Down)
            {
                SendDirection(Direction.DOWN);
                godown = true;
                pacman.Image = Properties.Resources.down;
            }
            if (e.KeyCode == Keys.Enter)
            {
                tbMsg.Enabled = true; tbMsg.Focus();
            }
        }

        private void Keyisup(object sender, KeyEventArgs e)
        {
            if (
                (e.KeyCode == Keys.Left && lastSentDirection == Direction.RIGHT) ||
                (e.KeyCode == Keys.Right && lastSentDirection == Direction.LEFT) ||
                (e.KeyCode == Keys.Up && lastSentDirection == Direction.DOWN) ||
                (e.KeyCode == Keys.Down && lastSentDirection == Direction.UP))
            {
                SendDirection(Direction.NONE);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
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
            if (redGhost.Bounds.IntersectsWith(wallPictureBoxes[0].Bounds))
                ghost1 = -ghost1;
            // if the red ghost hits the wall 2 we reverse the speed
            else if (redGhost.Bounds.IntersectsWith(wallPictureBoxes[1].Bounds))
                ghost1 = -ghost1;
            // if the yellow ghost hits the wall 3 then we reverse the speed
            if (yellowGhost.Bounds.IntersectsWith(wallPictureBoxes[2].Bounds))
                ghost2 = -ghost2;
            // if the yellow chost hits the wall 4 then wereverse the speed
            else if (yellowGhost.Bounds.IntersectsWith(wallPictureBoxes[3].Bounds))
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
                        //timer1.Stop();
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
                            //timer1.Stop();
                        }
                    }
                }
            }
            pinkGhost.Left += ghost3x;
            pinkGhost.Top += ghost3y;

            if (pinkGhost.Left < boardLeft ||
                pinkGhost.Left > boardRight ||
                (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[0].Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[1].Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[2].Bounds)) ||
                (pinkGhost.Bounds.IntersectsWith(wallPictureBoxes[3].Bounds)))
            {
                ghost3x = -ghost3x;
            }
            if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
            {
                ghost3y = -ghost3y;
            }
        }

        private void TbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Console.WriteLine("Sending message {0}", tbMsg.Text);

                outManager.SendCommand(new Command
                {
                    Type = Server.CommandType.Chat,
                    Args = new ChatMessage
                    {
                        Sender = Pid,
                        Message = tbMsg.Text
                    }
                }, OutManager.CLIENT_BROADCAST);

                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        public void AppendMessageToChat(string message)
        {
            tbChat.Text += "\r\n" + message;
        }

        private void TbChat_MouseDown(object sender, MouseEventArgs e)
        {
            if (!tbChat.Focused)
                tbChat.Focus();
        }

        private void TbChat_TextChanged(object sender, EventArgs e)
        {
            this.tbChat.SelectionStart = this.tbChat.Text.Length;
            this.tbChat.ScrollToCaret();
        }

        private void MainFrame_Load(object sender, EventArgs e)
        {
        }
    }
}