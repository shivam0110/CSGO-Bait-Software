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
     PUNISHMENT: HotPotato
     DESCRIPTION: Automatically drop the bomb if the cheater is the last alive teammate and on a bomb site
    */
    class HotPotato : Punishment
    {
        public Weapons LastActiveWeapon;

        int AliveTeammates = 0;

        public HotPotato() : base(0, false) // 0 = Always active
        {

        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (!Player.IsAlive()) return;

                Weapons ActiveWeapon = (Weapons) Program.GameData.Player.ActiveWeapon;

                if(ActiveWeapon == Weapons.C4)
                {
                    if(GameData.MatchInfo.AliveTeammates == 0 && GameData.Player.Location.ToLower().Contains("bombsite"))
                    {
                        Program.GameConsole.SendCommand("drop");
                        base.AfterActivate();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "HotPotatoException",
                    AnalyticsLabel = ex.Message
                });
            }

        }

    }
}
