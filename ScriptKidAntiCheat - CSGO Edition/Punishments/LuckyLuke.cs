using ScriptKidAntiCheat.Utils;
using System;
using System.Threading;
using System.Timers;
using static ScriptKidAntiCheat.Utils.MouseHook;

namespace ScriptKidAntiCheat.Punishments
{
    class LuckyLuke : Punishment
    {
        public bool LuckyLukeModeIsActive = false;

        public bool HasChanged = false;

        public bool IsShooting = false;

        private bool IsAimingDownScope = false;

        public DateTime startTime;

        public DateTime endTime;

        public override int ActivateOnRound { get; set; } = 2;

        public LuckyLuke() : base(0, false, 1) // 0 = Always active
        {
            MouseHook.MouseAction += new EventHandler(Event);
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                LuckyLukeModeIsActive = false;

                Weapons PrimaryWeapon = (Weapons)Program.GameData.Player.PrimaryWeapon;

                if (PrimaryWeapon != Weapons.Awp && PrimaryWeapon != Weapons.Scout) return;

                // Read directly from memory
                Weapons ActiveWeapon = (Weapons)Program.GameData.Player.getActiveWeapon(Program.GameProcess);

                IsAimingDownScope = Program.GameData.Player.checkIfAds(Program.GameProcess);

                if (IsAimingDownScope == true && (ActiveWeapon == Weapons.Awp || ActiveWeapon == Weapons.Scout))
                {
                    LuckyLukeModeIsActive = true;
                }

                if(LuckyLukeModeIsActive != HasChanged)
                {
                    HasChanged = LuckyLukeModeIsActive;
                    if (LuckyLukeModeIsActive)
                    {
                        Program.GameConsole.SendCommand("unbind mouse1");
                    } else
                    {
                        Program.GameConsole.SendCommand("bind mouse1 +attack");
                    }
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

                // Read directly from memory 
                bool CanShoot = Program.GameData.Player.checkIfCanShoot(Program.GameProcess);

                if (CanShoot == false || LuckyLukeModeIsActive == false || IsAimingDownScope == false || IsShooting == true) return;

                // If Player release left mouse button (fire)
                if ((MouseEvents)MouseEvent == MouseEvents.WM_LBUTTONDOWN)
                {
                    ActivatePunishment();
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

            IsShooting = true;

            Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

            Thread.Sleep(25);

            Program.GameConsole.SendCommand("slot2");

            Thread.Sleep(25);

            switch (ActiveWeapon)
            {
                case Weapons.Awp:
                    {
                        Program.GameConsole.SendCommand("play weapons/awp/awp_01.wav");
                        break;
                    }
                case Weapons.Scout:
                    {
                        Program.GameConsole.SendCommand("play weapons/ssg08/ssg08_01.wav");
                        break;
                    }
                default: break;
            }

            Thread.Sleep(100);

            Program.GameConsole.SendCommand("slot1; -attack");

            IsShooting = false;

            base.AfterActivate();
        }
    }
}
