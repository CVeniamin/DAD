using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace OGP.Client
{
    partial class MainFrame
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.PictureBox redGhost;
        private System.Windows.Forms.PictureBox yellowGhost;
        private System.Windows.Forms.PictureBox pinkGhost;
        private System.Windows.Forms.PictureBox pacman;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;

        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.TextBox tbChat;

        private PictureBox[] coinsArray = Enumerable.Repeat(0, 48).Select(c => new System.Windows.Forms.PictureBox()).ToArray();

        private PictureBox[] wallsArray = Enumerable.Repeat(0, 4).Select(w => new System.Windows.Forms.PictureBox()).ToArray();

        private void drawCoins()
        {
            short coinPosX = 11;
            short coinPosY = 49;
            short tabIndex = 73;
            short columnsX = 1;
            for (int i = 0; i < coinsArray.Length; i++)
            {
                //completed one row, reset x and y positions to new row
                if (columnsX == 7)
                {
                    coinPosX = 11;
                    coinPosY += 49;
                    columnsX = 1;
                }

                coinsArray[i].Image = global::OGP.Client.Properties.Resources.cccc;
                coinsArray[i].Location = new System.Drawing.Point(coinPosX, coinPosY);
                coinsArray[i].Margin = new System.Windows.Forms.Padding(4);
                coinsArray[i].Name = "pictureBox" + i.ToString();
                coinsArray[i].Size = new System.Drawing.Size(20, 18);
                coinsArray[i].SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                coinsArray[i].TabIndex = tabIndex;
                coinsArray[i].TabStop = false;
                coinsArray[i].Tag = "coin";

                coinPosX += 80;
                columnsX++;
                tabIndex++;

                ((System.ComponentModel.ISupportInitialize)(coinsArray[i])).BeginInit();

            }
        }
        /// <summary>
        /// Method used to draw a ghost give a Object, name, resource and x,y 
        /// </summary>
        private void drawGhost(PictureBox ghost, string ghostname, Bitmap ghostResource, int x, int y)
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
        private void drawWall(PictureBox wall, string name, int[] pos)
        {
            wall.BackColor = Color.MidnightBlue;
            wall.Location = new Point(pos[0], pos[1]);
            wall.Margin = new Padding(4);
            wall.Name = name;
            wall.Size = new Size(20, 117);
            wall.SizeMode = PictureBoxSizeMode.Zoom;
            wall.TabIndex = 3;
            wall.TabStop = false;
            wall.Tag = "wall";

            ((System.ComponentModel.ISupportInitialize)(wall)).BeginInit();
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pacman = new System.Windows.Forms.PictureBox();
            this.pinkGhost = new System.Windows.Forms.PictureBox();
            this.yellowGhost = new System.Windows.Forms.PictureBox();
            this.redGhost = new System.Windows.Forms.PictureBox();

            int[] wall1 = new int[] { 117, 49 };
            int[] wall2 = new int[] { 331, 49 };
            int[] wall3 = new int[] { 171, 295 };
            int[] wall4 = new int[] { 384, 295 };

            int[][] walls = { wall1, wall2, wall3, wall4 };

            for (int i = 0; i < wallsArray.Length; i++)
            {
                drawWall(wallsArray[i], "wall" + i.ToString(), walls[i]);
            }

            this.tbMsg = new System.Windows.Forms.TextBox();
            this.tbChat = new System.Windows.Forms.TextBox();

            ((System.ComponentModel.ISupportInitialize)(this.pacman)).BeginInit();

            drawCoins();

            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 25);
            this.label1.TabIndex = 71;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(237, -1);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 39);
            this.label2.TabIndex = 72;
            this.label2.Text = "label2";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // pacman
            // 
            this.pacman.BackColor = System.Drawing.Color.Transparent;
            this.pacman.Image = global::OGP.Client.Properties.Resources.Left;
            this.pacman.Location = new System.Drawing.Point(11, 49);
            this.pacman.Margin = new System.Windows.Forms.Padding(0);
            this.pacman.Name = "pacman";
            this.pacman.Size = new System.Drawing.Size(33, 31);
            this.pacman.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pacman.TabIndex = 4;
            this.pacman.TabStop = false;

            drawGhost(this.pinkGhost, "pinkGhost", global::OGP.Client.Properties.Resources.pink_guy, 401, 89);
            drawGhost(this.yellowGhost, "yellowGhost", global::OGP.Client.Properties.Resources.yellow_guy, 295, 336);
            drawGhost(this.redGhost, "redGhost", global::OGP.Client.Properties.Resources.red_guy, 240, 90);
           
            //
            // tbMsg
            // 
            this.tbMsg.Enabled = false;
            this.tbMsg.Location = new System.Drawing.Point(489, 388);
            this.tbMsg.Margin = new System.Windows.Forms.Padding(4);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(132, 22);
            this.tbMsg.TabIndex = 143;
            this.tbMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbMsg_KeyDown);
            // 
            // tbChat
            // 
            this.tbChat.Enabled = false;
            this.tbChat.Location = new System.Drawing.Point(489, 49);
            this.tbChat.Margin = new System.Windows.Forms.Padding(4);
            this.tbChat.Multiline = true;
            this.tbChat.Name = "tbChat";
            this.tbChat.Size = new System.Drawing.Size(132, 313);
            this.tbChat.TabIndex = 144;
            // 
            // MainFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 478);
            this.Controls.Add(this.tbChat);
            this.Controls.Add(this.tbMsg);

            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pacman);

            this.Controls.Add(this.pinkGhost);
            this.Controls.Add(this.yellowGhost);
            this.Controls.Add(this.redGhost);

            void AddControlsFor(PictureBox[] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    this.Controls.Add(array[i]);
                }
            }

            AddControlsFor(wallsArray);
            AddControlsFor(coinsArray);

            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainFrame";
            this.Text = "DADman";
            this.Load += new System.EventHandler(this.MainFrame_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.keyisdown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.keyisup);
            ((System.ComponentModel.ISupportInitialize)(this.pacman)).EndInit();
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

            this.ResumeLayout(false);
            this.PerformLayout();

        }
    #endregion
    }
}