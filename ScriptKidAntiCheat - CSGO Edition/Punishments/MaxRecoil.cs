using ScriptKidAntiCheat.Classes;
using ScriptKidAntiCheat.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ScriptKidAntiCheat.Punishments
{
    class MaxRecoil : Punishment
    {
        public override int ActivateOnRound { get; set; } = 3;

        public static bool logDelay = false;

        public Weapons LastActiveWeapon;

        public int BulletCount { get; set; } = -1;

        private bool Dormant { get; set; } = false;

        private bool Active { get; set; } = false;

        public MaxRecoil() : base(0, true, 50) // 0 = Always active
        {
            BulletCount = Player.AmmoCount;
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {

                int AmmoInWeapon = Player.AmmoCount;

                if(LastActiveWeapon != (Weapons)Player.ActiveWeapon)
                {
                    LastActiveWeapon = (Weapons)Player.ActiveWeapon;
                    BulletCount = AmmoInWeapon;
                }

                if (BulletCount != AmmoInWeapon)
                {
                    BulletCount = AmmoInWeapon;

                    if (Dormant) return;

                    int rnd = new Random().Next(2);

                    // 33% chance to activate every 10 seconds when starting to shoot
                    if(rnd == 0)
                    {
                        Active = true;
                        Task.Run(() => {
                            Thread.Sleep(30000);
                            Active = false;
                        });
                    } else if(!Active)
                    {
                        Dormant = true;
                        Task.Run(() => {
                            Thread.Sleep(5000);
                            Dormant = false;
                        });
                    }

                    if(Active)
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

            for (int i = 0; i < 5; i++)
            {
                // Add Extreme Recoil
                Task.Run(() =>
                {
                    Random rnd = new Random();
                    SendInput.MouseMove(rnd.Next(-250, 250), rnd.Next(-250, 250));
                });
            }

            // Prevent flooding the replay log with multiple instances of this punishment in a row
            if (logDelay == false)
            {
                logDelay = true;
                base.AfterActivate();
                Task.Run(() => {
                    Thread.Sleep(10000);
                    logDelay = false;
                });
            }
        }

        override public void Reset()
        {
            Active = false;
            Dormant = false;
            BulletCount = -1;
        }

    }
}
