using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace matchagame
{
    public partial class Form1 : Form
    {
        private TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Timer revealTimer;
        private System.Windows.Forms.Timer stopwatchTimer;
        private Label timerLabel;
        private Panel menuPanel;
        private System.Windows.Forms.Timer colorFadeTimer;

        private List<string> icons;
        private Label firstClicked = null;
        private Label secondClicked = null;
        private int elapsedSeconds = 0;
        private bool gameRunning = false;
        private float hue = 210f;

        private int rows = 4;
        private int cols = 4;

        private enum Difficulty { Easy, Normal, Hard }
        private Difficulty currentDifficulty = Difficulty.Normal;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ShowMenu();
        }

        // main character
        private void ShowMenu()
        {
            this.Controls.Clear();
            gameRunning = false;

            menuPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.SteelBlue };

            Label title = new Label
            {
                Text = "🧩 Match Game",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 150,
                TextAlign = ContentAlignment.BottomCenter
            };

            // Create buttons
            Button easyBtn = CreateMenuButton("Easy", Difficulty.Easy);
            Button normalBtn = CreateMenuButton("Normal", Difficulty.Normal);
            Button hardBtn = CreateMenuButton("Hard", Difficulty.Hard);

            FlowLayoutPanel buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 20, 0, 0),
                BackColor = Color.Transparent
            };

            buttonRow.Controls.Add(easyBtn);
            buttonRow.Controls.Add(normalBtn);
            buttonRow.Controls.Add(hardBtn);

            // Center the row
            buttonRow.Width = easyBtn.Width + normalBtn.Width + hardBtn.Width + 40; // 20px spacing
            buttonRow.Left = (this.ClientSize.Width - buttonRow.Width) / 2;

            menuPanel.Controls.Add(title);
            menuPanel.Controls.Add(buttonRow);
            this.Controls.Add(menuPanel);

            // Background color fade
            colorFadeTimer = new System.Windows.Forms.Timer();
            colorFadeTimer.Interval = 40;
            colorFadeTimer.Tick += (s, e) =>
            {
                hue += 1.2f;
                if (hue > 360f) hue = 0f;
                menuPanel.BackColor = ColorFromHSV(hue, 0.45, 1);
            };
            colorFadeTimer.Start();
        }

        // Helper to create buttons with hover effect
        private Button CreateMenuButton(string text, Difficulty diff)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 160,
                Height = 60,
                BackColor = Color.White,
                ForeColor = Color.SteelBlue,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { currentDifficulty = diff; SetupGame(); };

            // Hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = Color.LightGray;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.White;

            return btn;
        }


        // random crap go!
        private void SetupGame()
        {
            colorFadeTimer?.Stop();
            colorFadeTimer?.Dispose();

            this.Controls.Clear();
            gameRunning = true;
            firstClicked = null;
            secondClicked = null;
            elapsedSeconds = 0;

            // Set grid & icons by difficulty
            switch (currentDifficulty)
            {
                case Difficulty.Easy:
                    rows = 3; cols = 4;
                    icons = new List<string> { "!", "!", "N", "N", ",", ",", "k", "k", "b", "b", "v", "v" }; // 6 pairs
                    break;
                case Difficulty.Normal:
                    rows = 4; cols = 4;
                    icons = new List<string> { "!", "!", "N", "N", ",", ",", "k", "k", "b", "b", "v", "v", "w", "w", "z", "z" }; // 8 pairs
                    break;
                case Difficulty.Hard:
                    rows = 5; cols = 4;
                    icons = new List<string> { "!", "!", "N", "N", ",", ",", "k", "k", "b", "b", "v", "v", "w", "w", "z", "z", "$", "$", "@", "@" }; // 10 pairs
                    break;
            }

            // TableLayoutPanel
            tableLayoutPanel1 = new TableLayoutPanel
            {
                RowCount = rows,
                ColumnCount = cols,
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset
            };

            for (int r = 0; r < rows; r++) tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            for (int c = 0; c < cols; c++) tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

            this.Controls.Add(tableLayoutPanel1);

            // Timer label
            timerLabel = new Label
            {
                AutoSize = true,
                Padding = new Padding(10, 5, 10, 5),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = "Time: 0s",
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Dock = DockStyle.Top
            };
            this.Controls.Add(timerLabel);
            this.Controls.SetChildIndex(timerLabel, 0);

            // Timers
            revealTimer = new System.Windows.Forms.Timer();
            revealTimer.Interval = currentDifficulty == Difficulty.Hard ? 500 : 750; // faster hide for hard
            revealTimer.Tick += RevealTimer_Tick;

            stopwatchTimer = new System.Windows.Forms.Timer();
            stopwatchTimer.Interval = 1000;
            stopwatchTimer.Tick += StopwatchTimer_Tick;

            AssignIconsToSquares();
            stopwatchTimer.Start();
        }

        private void AssignIconsToSquares()
        {
            Random rand = new Random();
            var shuffled = icons.OrderBy(x => rand.Next()).ToList();
            int i = 0;

            tableLayoutPanel1.Controls.Clear();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (i >= shuffled.Count) break;

                    Label lbl = new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Webdings", 48, FontStyle.Bold),
                        BackColor = Color.CornflowerBlue,
                        ForeColor = Color.CornflowerBlue,
                        Text = shuffled[i]
                    };
                    lbl.Click += Label_Click;
                    tableLayoutPanel1.Controls.Add(lbl, c, r);
                    i++;
                }
            }
        }

        private void Label_Click(object sender, EventArgs e)
        {
            if (!gameRunning || revealTimer.Enabled) return;
            Label clickedLabel = sender as Label;
            if (clickedLabel == null || clickedLabel.ForeColor == Color.Black) return;

            clickedLabel.ForeColor = Color.Black;

            if (firstClicked == null) { firstClicked = clickedLabel; return; }

            secondClicked = clickedLabel;

            if (firstClicked.Text == secondClicked.Text)
            {
                AnimateMatch(firstClicked);
                AnimateMatch(secondClicked);
                firstClicked = null;
                secondClicked = null;
                CheckForWinner();
            }
            else
            {
                revealTimer.Start();
            }
        }

        private void RevealTimer_Tick(object sender, EventArgs e)
        {
            revealTimer.Stop();
            firstClicked.ForeColor = firstClicked.BackColor;
            secondClicked.ForeColor = secondClicked.BackColor;
            firstClicked = null;
            secondClicked = null;
        }

        private void StopwatchTimer_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
            timerLabel.Text = $"Time: {elapsedSeconds}s";
        }

        private void CheckForWinner()
        {
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                if (control is Label lbl && lbl.ForeColor == lbl.BackColor) return;
            }

            stopwatchTimer.Stop();
            gameRunning = false;

            MessageBox.Show(
            $"You matched all icons in {elapsedSeconds} seconds!",
            "Congratulations!!!!",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);    
            // Go back to menu
            ShowMenu();

        }

        private void AnimateMatch(Label lbl)
        {
            Color start = lbl.BackColor;
            Color end = Color.LightGreen;
            int steps = 10;
            int currentStep = 0;

            System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 30;

            fadeTimer.Tick += (s, e) =>
            {
                float t = currentStep / (float)steps;
                int r = (int)(start.R + (end.R - start.R) * t);
                int g = (int)(start.G + (end.G - start.G) * t);
                int b = (int)(start.B + (end.B - start.B) * t);
                lbl.BackColor = Color.FromArgb(r, g, b);

                currentStep++;
                if (currentStep > steps)
                {
                    lbl.BackColor = end;
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };

            fadeTimer.Start();
        }

        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q),
            };
        }
    }
}
