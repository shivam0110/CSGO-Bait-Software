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
     PUNISHMENT: NoSpray4U
     DESCRIPTION: Drop the cheaters weapons after spraying for a few bullets
    */
    class NoSpray4U : Punishment
    {
        public override int ActivateOnRound { get; set; } = 5;

        public NoSpray4U() : base(0) // 0 = Always active
        {
            Program.GameData.MatchInfo.OnMatchNewRound += OnNewRound;
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                Random rand = new Random();
                int DropAtBulletCount = rand.Next(5, 10);
                if (Player.IsAlive() && Player.BulletCounter(GameProcess) > DropAtBulletCount)
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
                    AnalyticsAction = "NoSpray4UException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        private void OnNewRound(object sender, EventArgs e)
        {
            Enabled = true;
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            Program.GameConsole.SendCommand("drop; drop;");

            base.AfterActivate();

            Random rand = new Random();
            if (rand.Next(0, 5) == 0)
            {
                Enabled = false;
            }

        }

    }
}
