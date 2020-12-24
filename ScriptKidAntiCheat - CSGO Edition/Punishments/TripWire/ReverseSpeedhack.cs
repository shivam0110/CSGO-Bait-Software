using Gma.System.MouseKeyHook;
using Gma.System.MouseKeyHook.Implementation;
using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: ReverseSpeedhack
     DESCRIPTION: Make the cheater move extremly slowly for 20 seconds
    */
    class ReverseSpeedhack : Punishment
    {
        private bool slowmode_activated = false;
        public bool w_key_down { get; set; } = false;
        public bool s_key_down { get; set; } = false;
        public bool a_key_down { get; set; } = false;
        public bool d_key_down { get; set; } = false;
        public int SlowMotionAmount { get; set; } = 95;

        public ReverseSpeedhack() : base(20000, true, 100)
        {
            LogEntry Entry = new LogEntry();
            Entry.LogTypes.Add(LogTypes.Replay);
            Entry.LogMessage = this.GetType().Name;
            Log.AddEntry(Entry);

            Program.m_GlobalHook.KeyDown += GlobalHookKeyDown;
            Program.m_GlobalHook.KeyUp += GlobalHookKeyUp;
        }

        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.W:
                    w_key_down = true;
                    break;
                case Keys.S:
                    s_key_down = true;
                    break;
                case Keys.A:
                    a_key_down = true;
                    break;
                case Keys.D:
                    d_key_down = true;
                    break;
            }

        }

        private void GlobalHookKeyUp(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.W:
                    w_key_down = false;
                    break;
                case Keys.S:
                    s_key_down = false;
                    break;
                case Keys.A:
                    a_key_down = false;
                    break;
                case Keys.D:
                    d_key_down = false;
                    break;
            }

        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (!w_key_down && !s_key_down && !a_key_down && !d_key_down) return;

                if (!slowmode_activated)
                {
                    ActivatePunishment();
                } else
                {
                    if (w_key_down && !s_key_down)
                    {
                        Task.Run(() =>
                        {
                            SendInput.KeyDown(KeyCode.KEY_S);
                            Thread.Sleep(SlowMotionAmount);
                            SendInput.KeyUp(KeyCode.KEY_S);
                        });
                    }

                    if (s_key_down && !w_key_down)
                    {
                        Task.Run(() =>
                        {
                            SendInput.KeyDown(KeyCode.KEY_W);
                            Thread.Sleep(SlowMotionAmount);
                            SendInput.KeyUp(KeyCode.KEY_W);
                        });
                    }

                    if (a_key_down && !d_key_down)
                    {
                        Task.Run(() =>
                        {
                            SendInput.KeyDown(KeyCode.KEY_D);
                            Thread.Sleep(SlowMotionAmount);
                            SendInput.KeyUp(KeyCode.KEY_D);
                        });
                    }

                    if (d_key_down && !a_key_down)
                    {
                        Task.Run(() =>
                        {
                            SendInput.KeyDown(KeyCode.KEY_A);
                            Thread.Sleep(SlowMotionAmount);
                            SendInput.KeyUp(KeyCode.KEY_A);
                        });
                    }
                }

                
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "ReverseSpeedHackException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            slowmode_activated = true;

            Program.FakeCheat.DisableCSGOESCMenu = true;

            Program.GameConsole.SendCommand("sensitivity 0.2");

            base.AfterActivate();
        }

        override public void Reset()
        {
            slowmode_activated = false;
            Program.m_GlobalHook.KeyDown -= GlobalHookKeyDown;
            Program.m_GlobalHook.KeyUp -= GlobalHookKeyUp;
            Program.FakeCheat.DisableCSGOESCMenu = false;
            PlayerConfig.ResetConfig();
        }

    }
}
