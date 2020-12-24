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
     PUNISHMENT: HeavyKnife
     DESCRIPTION: Force cheater to walk while holding knife
    */
    class HeavyKnife : Punishment
    {
        public override int ActivateOnRound { get; set; } = 5;

        public bool IsActive = false;

        public bool StatusChange = false;

        public static bool logDelay = false;

        public HeavyKnife() : base(0) // 0 = Always active
        {
            Program.GameConsole.SendCommand("-speed");
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                if (Player.IsAlive() && ( ActiveWeapon == Weapons.Knife_CT || ActiveWeapon == Weapons.Knife_T) )
                {
                    IsActive = true;
                } else
                {
                    IsActive = false;
                }

                if(StatusChange != IsActive)
                {
                    if(IsActive)
                    {
                        ActivatePunishment();
                    } else
                    {
                        Program.GameConsole.SendCommand("-speed");
                    }
                }

                StatusChange = IsActive;
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "HeavyKnifeException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            Program.GameConsole.SendCommand("+speed");

            // Prevent flooding the replay log with multiple instances of this punishment in a row
            if (logDelay == false)
            {
                logDelay = true;
                base.AfterActivate();
                Task.Run(() => {
                    Thread.Sleep(15000);
                    logDelay = false;
                });
            }
        }

    }
}
