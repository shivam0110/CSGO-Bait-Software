using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static ScriptKidAntiCheat.Utils.MouseHook;

namespace ScriptKidAntiCheat.Punishments
{
    class FlashInYourFace : Punishment
    {
        public bool IsThrowing = false;

        public Weapons CurrentWeapon { get; set; }

        public override int ActivateOnRound { get; set; } = 1;

        public FlashInYourFace() : base(0) // 0 = Always active
        {
            MouseHook.MouseAction += new EventHandler(Event);
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                if (CurrentWeapon == ActiveWeapon) return;

                CurrentWeapon = ActiveWeapon;

                // If player is holding flashbang or smoke
                if (CurrentWeapon == Weapons.Flashbang || CurrentWeapon == Weapons.Smoke)
                {
                    Program.GameConsole.SendCommand("bind mouse1 +attack2");
                } else
                {
                    Program.GameConsole.SendCommand("bind mouse1 +attack");
                }
            }
            catch (Exception ex)
            {
                // yeet
            }

        }

        private void Event(object MouseEvent, EventArgs e)
        {
            try
            {
                if (!Program.GameProcess.IsValidAndActiveWindow || !Program.GameData.Player.IsAlive() || Program.GameData.MatchInfo.isFreezeTime) return;

                if (Player.CanShoot == false || IsThrowing) return;

                // If Player release left mouse button (fire)
                if ((MouseEvents)MouseEvent == MouseEvents.WM_LBUTTONUP)
                {
                    Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                    // If player is throwing flashbang
                    if (ActiveWeapon == Weapons.Flashbang)
                    {
                        ActivatePunishment();
                    }
                }
            }
            catch (Exception ex)
            {
                // yeet
            }
        }

        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            Program.GameConsole.SendCommand("sensitivity 0.1");

            Task.Run(() =>
            {
                Thread.Sleep(2000);
                PlayerConfig.ResetConfig();
            });

            base.AfterActivate();
        }

    }
}
