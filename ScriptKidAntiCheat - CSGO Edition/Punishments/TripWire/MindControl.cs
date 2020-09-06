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
    class MindControl : Punishment
    {

        public MindControl(List<MindControlAction> Actions, bool FakeFlash = true, int FakeFlashDuration = 2000) : base(0, true, 50)
        {
            if (Actions == null) return;

            try
            {
                if (base.CanActivate() == false) return;

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

                base.AfterActivate();
            }
            catch (Exception ex)
            {
                // yeet
            }

        }

    }
}
