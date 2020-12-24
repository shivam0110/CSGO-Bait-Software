using ScriptKidAntiCheat.Classes;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: BigSpender
     DESCRIPTION: Waste cheaters economy buy force buying bad weapons
    */
    class BigSpender : Punishment
    {
        public bool isBuying = false;

        public override int ActivateOnRound { get; set; } = 3;

        public BigSpender() : base(0, false, 500) // 0 = Always active
        {
         
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (Player.IsAlive() && GameData.MatchInfo.isFreezeTime == true && isBuying == false)
                {
                    ActivatePunishment();
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "BigSpenderException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            isBuying = true;

            Task.Run(() => {
                Thread.Sleep(500);
                Program.GameConsole.SendCommand("buy elite; drop; buy elite; drop;");
                Thread.Sleep(500);
                Program.GameConsole.SendCommand("buy nova; drop; buy nova; drop;");
                Thread.Sleep(500);
                Program.GameConsole.SendCommand("buy mac10; drop; buy mac10; drop;");
            });

            base.AfterActivate();

            this.Dispose();

        }

    }
}
