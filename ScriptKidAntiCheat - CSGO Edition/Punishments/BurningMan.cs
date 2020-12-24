﻿using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static ScriptKidAntiCheat.Utils.MouseHook;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: BurningMan
     DESCRIPTION: Cause the cheater to throw molly and nades straight down
    */
    class BurningMan : Punishment
    {
        public bool IsThrowing = false;

        public string TypeOfNade = "";

        public override int ActivateOnRound { get; set; } = 5;

        public BurningMan() : base(0) // 0 = Always active
        {
            MouseHook.MouseAction += new EventHandler(Event);
        }

        private void Event(object MouseEvent, EventArgs e) {
            try
            {
                if (!Program.GameProcess.IsValidAndActiveWindow || !Program.GameData.Player.IsAlive() || Program.GameData.MatchInfo.isFreezeTime) return;

                if (Player.CanShoot == false || IsThrowing) return;

                // If Player release left mouse button (fire)
                if ((MouseEvents)MouseEvent == MouseEvents.WM_LBUTTONUP)
                {
                    Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                    // If player is throwing nade, molotov or incendiary
                    if(ActiveWeapon == Weapons.Molotov || ActiveWeapon == Weapons.Incendiary || ActiveWeapon == Weapons.Grenade)
                    {

                        if(ActiveWeapon == Weapons.Grenade)
                        {
                            TypeOfNade = "HE";
                        } else
                        {
                            TypeOfNade = "MO";
                        }

                        ActivatePunishment();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "BurningManException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            IsThrowing = true;

            Program.FakeCheat.DisableCSGOESCMenu = true;

            Task.Run(() =>
            {

                Thread.Sleep(75); // wait 75ms

                // Look down
                SendInput.MouseMove(0, 10000);

                Program.GameConsole.SendCommand("-forward; -back; -moveleft; -moveright; unbind W; unbind A; unbind S; unbind D");

                Thread.Sleep(2000); // unbind for 2 seconds :D

                PlayerConfig.ResetConfig();

                IsThrowing = false;

                Thread.Sleep(4000);

                Program.FakeCheat.DisableCSGOESCMenu = false;
            });

            base.AfterActivate(true, this.GetType().Name + "(" + TypeOfNade + ")");
        }

    }
}
