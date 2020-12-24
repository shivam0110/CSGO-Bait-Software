using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: NoCrosshairChallenge
     DESCRIPTION: Remove the crosshair for 20 seconds
    */
    class NoCrosshairChallenge : Punishment
    {

        public NoCrosshairChallenge() : base(20000, true) // 0 = Always active
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
                    AnalyticsAction = "NoCrosshairChallengeException",
                    AnalyticsLabel = ex.Message
                });
            }
            
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            Program.GameConsole.SendCommand("crosshair 0");

            base.AfterActivate();
        }

        override public void Reset()
        {
            Program.GameConsole.SendCommand("crosshair 1");
        }

    }
}
