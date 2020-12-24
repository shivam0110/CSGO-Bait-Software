using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: ViolenceSpeedMomentum
     DESCRIPTION: Give the cheater 100 sensitivity for 15 seconds
    */
    class ViolenceSpeedMomentum : Punishment
    {

        public ViolenceSpeedMomentum() : base(15000, true) // 0 = Always active
        {
            try
            {
                ActivatePunishment();
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "ViolenceSpeedMomentumException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            // Set in-game sensitivity to 100
            Program.GameConsole.SendCommand("sensitivity 100");

            // Jump 3 times
            Task.Run(() => {
                for (int i = 0; i < 3; i++)
                {
                    Program.GameConsole.SendCommand("+jump");
                    Thread.Sleep(100);
                    Program.GameConsole.SendCommand("-jump");
                    Thread.Sleep(1000);
                }
            });

            base.AfterActivate();
        }

        override public void Reset()
        {
            // TODO get player default sens
            //Program.GameConsole.SendCommand("sensitivity 0.8;");
            //Program.GameConsole.SendCommand("exec reset.cfg");
            PlayerConfig.ResetConfig();
        }

    }
}
