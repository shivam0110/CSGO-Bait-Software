using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using ScriptKidAntiCheat.Utils;
using WMPLib;
using Gma.System.MouseKeyHook;
using System.IO;

namespace ScriptKidAntiCheat
{
    public partial class FakeCheatForm : Form
    {

        private PictureBox title = new PictureBox(); // create a PictureBox
        private Label close = new Label(); // simulates the this.close box

        private bool drag = false; // determine if we should be moving the form
        private Point startPoint = new Point(0, 0); // also for the moving


        public FakeCheatForm()
        {

            InitializeComponent();
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.title.Location = this.Location; 
            this.title.Width = this.Width;
            this.title.Height = 30;
            this.title.BackColor = Color.Transparent;
            this.Controls.Add(this.title);
            this.title.BringToFront();

            this.pictureBox1.BringToFront();

            this.pictureBox1.MouseEnter += new EventHandler(CloseBtn_MouseEnter);
            this.pictureBox1.MouseLeave += new EventHandler(CloseBtn_MouseLeave);
            this.pictureBox1.MouseClick += new MouseEventHandler(CloseBtn_MouseClick);

            // finally, wouldn't it be nice to get some moveability on this control?
            this.title.MouseDown += new MouseEventHandler(Title_MouseDown);
            this.title.MouseUp += new MouseEventHandler(Title_MouseUp);
            this.title.MouseMove += new MouseEventHandler(Title_MouseMove);

            this.button1.MouseEnter += new EventHandler(InjectBtn_MouseEnter);
            this.button1.MouseLeave += new EventHandler(InjectBtn_MouseLeave);

            this.pictureBox3.MouseClick += new MouseEventHandler(FakeMenuClick);

            pictureBox2.BringToFront();

            // Set last check vac detected to todays date :D
            DateTime today = DateTime.Today;
            label4.Text = label4.Text + " " + String.Format("{0:yyyy-MM-dd}", today);

            // Set version number
            label5.Text = label5.Text + " " + Program.version;

            label1.BringToFront();


        }

        private void CloseBtn_MouseEnter(object sender, EventArgs e)
        {
                this.pictureBox1.BackColor = Color.LightBlue;
        }

        private void CloseBtn_MouseLeave(object sender, EventArgs e)
        {
                this.pictureBox1.BackColor = Color.Transparent;
        }

        private void CloseBtn_MouseClick(object sender, MouseEventArgs e)
        {
                this.Close();
        }

        private void InjectBtn_MouseEnter(object sender, EventArgs e)
        {
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(255)))));
        }

        private void InjectBtn_MouseLeave(object sender, EventArgs e)
        {
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(115)))), ((int)(((byte)(255)))));
        }


        void Title_MouseUp(object sender, MouseEventArgs e)
        {
            this.drag = false;
        }

        void Title_MouseDown(object sender, MouseEventArgs e)
        {
            this.startPoint = e.Location;
            this.drag = true;
        }

        void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.drag)
            { // if we should be dragging it, we need to figure out some movement
                Point p1 = new Point(e.X, e.Y);
                Point p2 = this.PointToScreen(p1);
                Point p3 = new Point(p2.X - this.startPoint.X,
                                     p2.Y - this.startPoint.Y);
                this.Location = p3;
            }
        }

        private void setCheckBoxColor(Object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if(cb.Checked)
            {
                cb.ForeColor = Color.LawnGreen;
            } else
            {
                cb.ForeColor = Color.Red;
            }
        }

        public void log(string msg)
        {
            Console.WriteLine(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Program.GameProcess != null && Program.GameProcess.Process != null && Program.GameProcess.Process.IsRunning())
            {
                if (!Program.GameData.MatchInfo.IsMatchmaking && !Program.Debug.AllowLocal)
                {
                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        AnalyticsCategory = "InjectButton",
                        AnalyticsAction = "NotInMatchmaking"
                    });
                    MessageBox.Show("You need to join a matchmaking game before injecting.", "No match in progress", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            else
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "InjectButton",
                    AnalyticsAction = "NotRunningCSGO"
                });
                MessageBox.Show("CSGO is not running!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void FakeMenuClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("You need to inject dll first", "Not injected", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
