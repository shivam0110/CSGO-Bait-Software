using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static ScriptKidAntiCheat.Utils.MouseHook;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT:    NadeKing
     DESCRIPTION:   When the cheater is trying to throw a smoke while on a bombsite (only for terrorists) 
                    it will instantly fake flash them + throw the smoke straight down + unbind their movement while fake flashed
                    + throw their weapons away in different directions out of the smoke
    */
    class NadeKing : Punishment
    {
        public bool IsThrowing = false;

        public override int ActivateOnRound { get; set; } = 5;

        private bool ActivatedOnCTSide = false;

        private bool ActivatedOnTSide = false;

        public NadeKing() : base(0) // 0 = Always active
        {
            MouseHook.MouseAction += new EventHandler(Event);

        }

        private void Event(object MouseEvent, EventArgs e)
        {
            try
            {
                if (!Program.GameProcess.IsValidAndActiveWindow || !Program.GameData.Player.IsAlive() || Program.GameData.MatchInfo.isFreezeTime) return;

                if (Player.CanShoot == false || IsThrowing) return;

                /*
                // Activate only once while playing ct
                if (ActivatedOnCTSide && Program.GameData.Player.Team == Team.CounterTerrorists) return;

                // Activate only once while playing t
                if (ActivatedOnTSide && Program.GameData.Player.Team == Team.Terrorists) return;
                */

                // If Player press left mouse button (fire)
                if ((MouseEvents)MouseEvent == MouseEvents.WM_LBUTTONDOWN)
                {
                    Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                    // If player is on bombsite and is about to throw smoke (trigger only for terrorists)
                    if (ActiveWeapon == Weapons.Smoke && !Helper.PlayerIsInSpawn())
                    {
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
                    AnalyticsAction = "NadeKingException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            IsThrowing = true;

            if (!ActivatedOnCTSide)
            {
                ActivatedOnCTSide = Program.GameData.Player.Team == Team.CounterTerrorists ? true : false;
            }

            if(!ActivatedOnTSide)
            {
                ActivatedOnTSide = Program.GameData.Player.Team == Team.Terrorists ? true : false;
            }

            // Throw smoke straight down then throw away weapons
            Task.Run(() =>
            {

                // Look down
                SendInput.MouseMove(0, 10000);

                Thread.Sleep(100);

                Vector3 PlayerPos = Program.GameData.Player.EyePosition;
                PlayerPos.X = PlayerPos.X - 1000;

                List<MindControlAction> MindControlActions = new List<MindControlAction>();
                MindControlActions.Add(new MindControlAction { Sleep = 250 });
                MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = PlayerPos, AimLockDuration = 250 });
                MindControlActions.Add(new MindControlAction { Sleep = 500 });
                MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop;" });
                MindControlActions.Add(new MindControlAction { Sleep = 500 });
                MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop;" });
                MindControlActions.Add(new MindControlAction { Sleep = 500 });
                MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop;" });
                Punishment p = new MindControl(MindControlActions, true, 4000, false);

                IsThrowing = false;
            });

            // Spin & Drop Weapons
            Task.Run(() =>
            {
                Thread.Sleep(300);

                // Spin while dropping weapons out of smoke
                var startTime = DateTime.UtcNow;
                int spinTime = 2;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(spinTime))
                {
                    var rand = new Random();
                    SendInput.MouseMove(rand.Next(300, 600), 0);
                    Thread.Sleep(10);
                }

                // After spinning is done lets keep dropping any weapons they pick up
                var startTime2 = DateTime.UtcNow;
                int dropWeaponTime = 60;
                while (DateTime.UtcNow - startTime2 < TimeSpan.FromSeconds(dropWeaponTime))
                {
                    Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                    if (Player.IsAlive() == false || Program.GameData.MatchInfo.isFreezeTime) return;

                    if (ActiveWeapon != Weapons.Knife_CT && 
                        ActiveWeapon != Weapons.Knife_T && 
                        ActiveWeapon != Weapons.Flashbang && 
                        ActiveWeapon != Weapons.Smoke && 
                        ActiveWeapon != Weapons.Grenade && 
                        ActiveWeapon != Weapons.Incendiary) {
                        Program.GameConsole.SendCommand("drop");
                    }
                   Thread.Sleep(500);
                }
            });

            base.AfterActivate();
        }

    }
}
