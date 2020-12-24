using ScriptKidAntiCheat.Classes;
using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: MindControl
     DESCRIPTION: Take full control of cheater (unbind their movement) and make them do stuff
    */
    class MindControl : Punishment
    {

        public MindControl(List<MindControlAction> Actions, bool FakeFlash = true, int FakeFlashDuration = 2000, bool logging = true) : base(0, true, 50)
        {
            if (Actions == null) return;

            try
            {
                if (base.CanActivate() == false) return;

                Program.FakeCheat.DisableCSGOESCMenu = true;

                Task.Run(() =>
                {

                    // Unbind movements
                    Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright; unbind w; unbind a; unbind s; unbind d;");

                    if(FakeFlash)
                    {
                        FakeFlash Flash = new FakeFlash(FakeFlashDuration);
                    }

                    foreach(MindControlAction Action in Actions)
                    {
                        if(Action.AimLockAtWorldPoint != default)
                        {
                            Helper.AimLock(Action.AimLockAtWorldPoint, Action.AimLockDuration);
                        }
                        if(Action.ConsoleCommand != "")
                        {
                            Program.GameConsole.SendCommand(Action.ConsoleCommand);
                        }
                        if (Action.Sleep != 0)
                        {
                            Thread.Sleep(Action.Sleep);
                        }
                    }

                    Thread.Sleep(100);

                    Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright; -jump; -duck; bind w +forward; bind s +back; bind a +moveleft; bind d +moveright;");

                });

                Program.FakeCheat.DisableCSGOESCMenu = false;

                base.AfterActivate(logging);
            }
            catch (Exception ex)
            {
                if(logging)
                {
                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        AnalyticsCategory = "Error",
                        AnalyticsAction = "MindControlException",
                        AnalyticsLabel = ex.Message
                    });
                }
            }

        }

    }
}
