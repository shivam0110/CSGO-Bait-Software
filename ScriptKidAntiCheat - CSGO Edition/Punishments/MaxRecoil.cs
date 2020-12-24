using ScriptKidAntiCheat.Classes;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Utils.Maths;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT:    MaxRecoil
     DESCRIPTION:   Randomly make the recoil extremly hard to control throughout the match
    */
    class MaxRecoil : Punishment
    {
        public override int ActivateOnRound { get; set; } = 3;
        public override bool DisposeOnReset { get; set; } = false;

        public static bool logDelay = false;

        public Weapons LastActiveWeapon;
        public int BulletCount { get; set; } = -1;

        private bool AimedAtEnemy { get; set; } = false;

        private DateTime startTime;

        private DateTime endTime;

        public MaxRecoil() : base(0, true, 50) // 0 = Always active
        {
            BulletCount = Player.AmmoCount;
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (Program.GameProcess.IsValidAndActiveWindow == false || Player.IsAlive() == false || base.CanActivate() == false || Program.GameData.MatchInfo.isFreezeTime)
                {
                    return;
                }

                if(IsAimingAtEnemy())
                {
                    startTime = DateTime.Now;
                    AimedAtEnemy = true;
                }

                Double elapsedMillisecs = ((TimeSpan)(DateTime.Now - startTime)).TotalSeconds;
                if (elapsedMillisecs > 3)
                {
                    AimedAtEnemy = false;
                }

                int AmmoInWeapon = Player.AmmoCount;

                if(LastActiveWeapon != (Weapons)Player.ActiveWeapon)
                {
                    LastActiveWeapon = (Weapons)Player.ActiveWeapon;
                    BulletCount = AmmoInWeapon;
                }

                if (BulletCount != AmmoInWeapon && AimedAtEnemy)
                {
                    BulletCount = AmmoInWeapon;
                    ActivatePunishment();
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "MaxRecoilException",
                    AnalyticsLabel = ex.Message
                });
            }
        }
        
        public bool IsAimingAtEnemy()
        {
            Vector3 AimDirection = Player.AimDirection;
            Vector3 GetPlayerLocation = Player.EyePosition;

            // get aim ray in world
            if (AimDirection.Length() < 0.001)
            {
                return false;
            }

            var aimRayWorld = new Line3D(GetPlayerLocation, GetPlayerLocation + AimDirection * 8192);

            // Go through entities
            foreach (var entity in GameData.Entities)
            {

                if (!entity.IsAlive() || entity.AddressBase == Player.AddressBase)
                {
                    continue;
                }

                if (entity.Team == Player.Team)
                {
                    continue;
                }

                if (entity.Spotted == false)
                {
                    continue;
                }

                // Check if hitbox intersect with aimRay
                var hitBoxId = Helper.IntersectsHitBox(aimRayWorld, entity);
                if (hitBoxId >= 0)
                {
                    return true;
                }

                Thread.Sleep(10);
            }

            return false;
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
            BulletCount = -1;
            AimedAtEnemy = false;
        }

    }
}
