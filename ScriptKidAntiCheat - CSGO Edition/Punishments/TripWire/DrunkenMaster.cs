using Gma.System.MouseKeyHook;
using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: DrunkenMaster
     DESCRIPTION: Randomize movement keys for 15 seconds
    */
    class DrunkenMaster : Punishment
    {

        private IKeyboardMouseEvents m_GlobalHook;

        private Keys LastDirection;

        public DrunkenMaster() : base(15000, true) // 0 = Always active
        {
            try
            {
                ActivatePunishment();
                Program.m_GlobalHook.KeyDown += GlobalHookKeyDown;
                Program.m_GlobalHook.KeyUp += GlobalHookKeyUp;

            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "DrunkenMasterException1",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LastDirection == e.KeyCode) return; 

                if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D)
                {
                    LastDirection = e.KeyCode;

                    Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright");

                    int rnd = new Random().Next(4);

                    if(rnd == 0)
                    {
                        Program.GameConsole.SendCommand("+forward");
                    } else if(rnd == 1)
                    {
                        Program.GameConsole.SendCommand("+back");
                    }
                    else if (rnd == 2)
                    {
                        Program.GameConsole.SendCommand("+moveleft");
                    }
                    else if (rnd == 3)
                    {
                        Program.GameConsole.SendCommand("+moveright");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "DrunkenMasterException2",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        private void GlobalHookKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D)
                {
                    LastDirection = default;
                    Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright");
                }
                
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "DrunkenMasterException3",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright");

            base.AfterActivate();
        }

        override public void Reset()
        {
            Program.m_GlobalHook.KeyDown -= GlobalHookKeyDown;
            Program.m_GlobalHook.KeyUp -= GlobalHookKeyUp;
            Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright");
        }

    }
}
