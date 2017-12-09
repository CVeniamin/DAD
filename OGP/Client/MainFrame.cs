using OGP.Middleware;
using OGP.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private int score;

        private Direction lastSentDirection = Direction.NONE;

        private Size coinSize;
        private Size ghostSize;
        private Size playerSize;

        private PictureBox[] coinPictureBoxes;
        private PictureBox[] wallPictureBoxes;
        private Dictionary<string, PictureBox> playerPictureBoxes;
        private bool pictureBoxesReady = false;

        private PictureBox player;
        private PictureBox pacman = new PictureBox();
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

            GameStatusLabel.Visible = true;
            GameStatusLabel.Text = "Waiting for players...";
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

            DrawGhosts(); // Draw once
            DrawWalls(gameStateView.Walls); // Draw once
            DrawCoins(gameStateView.Coins); // Draw once

            pictureBoxesReady = true;
        }

        internal void UpdateScore(GameStateView gameStateView)
        {
            foreach(Player p in gameStateView.Players)
            {
                if(p.PlayerId == this.Pid)
                {
                    this.score = p.Score;

                    ScoreLabel.SetPropertyThreadSafe(() => ScoreLabel.Text, "Score: " + score.ToString());
                    //ScoreLabel.Text = "Player " + this.Pid + " Score: " + score.ToString();
                }
            }
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

            if(gameStateView.RoundId > 0)
            {
                GameStatusLabel.SetPropertyThreadSafe(() => this.GameStatusLabel.Text, "Game On!");
            }

            if (gameStateView.GameOver)
            {
                GameStatusLabel.SetPropertyThreadSafe(() => this.GameStatusLabel.Text, "Game Over!");
            }

            //UpdateScore(gameStateView);
            UpdateCoins(gameStateView);
            UpdateGhosts(gameStateView);
            UpdatePlayers(gameStateView);
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

        private void UpdatePlayers(GameStateView gameStateView)
        {
            foreach (Player player in gameStateView.Players)
            {
                if (!playerPictureBoxes.TryGetValue(player.PlayerId, out PictureBox pictureBox))
                {
                    pictureBox = CreatePlayerPictureBox();
                    InitializeDrawing(pictureBox);
                    if(player.PlayerId == Pid)
                    {
                        this.pacman = pictureBox;
                    }
                    playerPictureBoxes.Add(player.PlayerId, pictureBox);
                }

                // Update Player Score
                if (player.PlayerId == this.Pid)
                {
                    ScoreLabel.SetPropertyThreadSafe(() => ScoreLabel.Text, String.Format("Player {0} Score: {1}", Pid, player.Score.ToString()));
                }

                if (player.Alive)
                {
                    Bitmap updatedBitmap = DirectionToResource(player.Direction);
                    if (updatedBitmap != null && pictureBox.Image != updatedBitmap)
                    {
                        pictureBox.Image = updatedBitmap;
                    }

                    pictureBox.Location = new Point(player.X, player.Y);
                }
                else
                {
                    pictureBox.Visible = false;
                    GameStatusLabel.SetPropertyThreadSafe(() => GameStatusLabel.Text, "Game Over!");
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

        private void SendDirection(Direction direction)
        {
            if (direction != lastSentDirection)
            {
                Console.WriteLine("SEND NEW DIRECTION!");
                lastSentDirection = direction;

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
                //pacman.Image = Properties.Resources.Left;
            }
            if (e.KeyCode == Keys.Right)
            {
                SendDirection(Direction.RIGHT);
                //pacman.Image = Properties.Resources.Right;
            }
            if (e.KeyCode == Keys.Up)
            {
                SendDirection(Direction.UP);
                //pacman.Image = Properties.Resources.Up;
            }
            if (e.KeyCode == Keys.Down)
            {
                SendDirection(Direction.DOWN);
                //pacman.Image = Properties.Resources.down;
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

        private void TbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; //don't write anything to tbMsg
                if (!String.IsNullOrWhiteSpace(tbMsg.Text))
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
                }

                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        public void AppendMessageToChat(string message)
        {
            tbChat.SetPropertyThreadSafe(() => this.tbChat.Text, this.tbChat.Text += message + "\r\n");
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