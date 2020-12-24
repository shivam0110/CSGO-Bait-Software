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
    public partial class Conditions : Form
    {

        private PictureBox title = new PictureBox(); // create a PictureBox
        private Label close = new Label(); // simulates the this.close box
        public bool acceptedTerms = false;
        private bool drag = false; // determine if we should be moving the form
        private Point startPoint = new Point(0, 0); // also for the moving

        public Conditions()
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

            pictureBox2.BringToFront();

            label1.BringToFront();

            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                IncludeTimeAndTick = false,
                AnalyticsCategory = "Conditions",
                AnalyticsAction = "Shown"
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (acceptedTerms == false)
            {
                Environment.Exit(0); // No "cheat" for you :(
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Conditions",
                    AnalyticsAction = "AcceptedTerms"
                });
                acceptedTerms = true;
                this.Close();
            }
            else
            {
                checkBox1.ForeColor = Color.Red;
                System.Windows.Forms.MessageBox.Show("You must accept the terms and conditions if you want to use this software.", "Terms & Conditions", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
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
    }
}
