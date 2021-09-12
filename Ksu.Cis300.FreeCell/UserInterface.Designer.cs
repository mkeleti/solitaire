
namespace Ksu.Cis300.FreeCell
{
    partial class UserInterface
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserInterface));
            this.uxToolBar = new System.Windows.Forms.ToolStrip();
            this.uxNewGame = new System.Windows.Forms.ToolStripButton();
            this.uxGameLabel = new System.Windows.Forms.ToolStripLabel();
            this.uxMoveHome = new System.Windows.Forms.ToolStripButton();
            this.uxGameNumber = new System.Windows.Forms.NumericUpDown();
            this.uxBoard = new Ksu.Cis300.FreeCell.DrawingBoard();
            this.uxToolBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxGameNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // uxToolBar
            // 
            this.uxToolBar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.uxToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uxNewGame,
            this.uxGameLabel,
            this.uxMoveHome});
            this.uxToolBar.Location = new System.Drawing.Point(0, 0);
            this.uxToolBar.Name = "uxToolBar";
            this.uxToolBar.Size = new System.Drawing.Size(800, 34);
            this.uxToolBar.TabIndex = 0;
            this.uxToolBar.Text = "toolStrip1";
            // 
            // uxNewGame
            // 
            this.uxNewGame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.uxNewGame.Image = ((System.Drawing.Image)(resources.GetObject("uxNewGame.Image")));
            this.uxNewGame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.uxNewGame.Name = "uxNewGame";
            this.uxNewGame.Size = new System.Drawing.Size(102, 29);
            this.uxNewGame.Text = "New Game";
            this.uxNewGame.Click += new System.EventHandler(this.uxNewGame_Click);
            // 
            // uxGameLabel
            // 
            this.uxGameLabel.Name = "uxGameLabel";
            this.uxGameLabel.Size = new System.Drawing.Size(132, 29);
            this.uxGameLabel.Text = "Game Number:";
            // 
            // uxMoveHome
            // 
            this.uxMoveHome.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.uxMoveHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.uxMoveHome.Enabled = false;
            this.uxMoveHome.Image = ((System.Drawing.Image)(resources.GetObject("uxMoveHome.Image")));
            this.uxMoveHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.uxMoveHome.Name = "uxMoveHome";
            this.uxMoveHome.Size = new System.Drawing.Size(140, 29);
            this.uxMoveHome.Text = "Move All Home";
            // 
            // uxGameNumber
            // 
            this.uxGameNumber.Location = new System.Drawing.Point(169, 3);
            this.uxGameNumber.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.uxGameNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.uxGameNumber.Name = "uxGameNumber";
            this.uxGameNumber.Size = new System.Drawing.Size(67, 20);
            this.uxGameNumber.TabIndex = 1;
            this.uxGameNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.uxGameNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // uxBoard
            // 
            this.uxBoard.BackColor = System.Drawing.Color.DarkGreen;
            this.uxBoard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uxBoard.Enabled = false;
            this.uxBoard.Location = new System.Drawing.Point(0, 34);
            this.uxBoard.Margin = new System.Windows.Forms.Padding(1);
            this.uxBoard.Name = "uxBoard";
            this.uxBoard.Size = new System.Drawing.Size(800, 416);
            this.uxBoard.TabIndex = 2;
            this.uxBoard.Paint += new System.Windows.Forms.PaintEventHandler(this.uxBoard_Paint);
            this.uxBoard.MouseClick += new System.Windows.Forms.MouseEventHandler(this.uxBoard_MouseClick);
            this.uxBoard.Resize += new System.EventHandler(this.uxBoard_Resize);
            // 
            // UserInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.uxBoard);
            this.Controls.Add(this.uxGameNumber);
            this.Controls.Add(this.uxToolBar);
            this.DoubleBuffered = true;
            this.Name = "UserInterface";
            this.Text = "FreeCell";
            this.uxToolBar.ResumeLayout(false);
            this.uxToolBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uxGameNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip uxToolBar;
        private System.Windows.Forms.ToolStripButton uxNewGame;
        private System.Windows.Forms.ToolStripLabel uxGameLabel;
        private System.Windows.Forms.NumericUpDown uxGameNumber;
        private DrawingBoard uxBoard;
        private System.Windows.Forms.ToolStripButton uxMoveHome;
    }
}

