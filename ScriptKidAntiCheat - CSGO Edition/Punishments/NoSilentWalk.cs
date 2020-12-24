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
     PUNISHMENT: NoSilentWalk
     DESCRIPTION: When the cheater is last alive and sneaking (walking) for over 10 seconds it will play taser or knife sound to scare him
    */
    class NoSilentWalk : Punishment
    {
        private static System.Timers.Timer punishmentTimer;

        private bool HasPlayedZeusSoundThisRound = false;

        private bool HasPlayedKnifeSoundThisRound = false;
        public override bool DisposeOnReset { get; set; } = false;

        public NoSilentWalk() : base(0, true) // 0 = Always active
        {
            // If player walks for more than 7 seconds lets trigger punishment
            punishmentTimer = new System.Timers.Timer(7000); 
            punishmentTimer.Elapsed += delayedPunishment;
            punishmentTimer.AutoReset = false;
            punishmentTimer.Enabled = false;
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

                if (Player.vecVelocity.X < 150 &&
                    Player.vecVelocity.X > -150 &&
                    Player.vecVelocity.X != 0 &&
                    Player.vecVelocity.Y < 150 &&
                    Player.vecVelocity.Y > -150 &&
                    Player.vecVelocity.Y != 0 &&
                    Player.IsAlive())
                {
                    // Player is walking or crouch walking
                    punishmentTimer.Start();
                }
                else
                {
                    // Reset timer if player is running
                    punishmentTimer.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "NoSilentWalkException",
                    AnalyticsLabel = ex.Message
                });
            }
            
        }

        private void delayedPunishment(Object source, ElapsedEventArgs e)
        {
            if (base.CanActivate() == false) return;

            if (HasPlayedZeusSoundThisRound && HasPlayedKnifeSoundThisRound) return;

            Task.Run(() =>
            {

                if (!HasPlayedZeusSoundThisRound)
                {
                    Program.GameConsole.SendCommand(@"play player/footsteps/new/land_tile_01.wav");
                    Thread.Sleep(500);
                    Program.GameConsole.SendCommand(@"play weapons\taser\taser_shoot");
                    HasPlayedZeusSoundThisRound = true;
                }
                else if(!HasPlayedKnifeSoundThisRound)
                {
                    Program.GameConsole.SendCommand(@"play weapons/knife/knife_deploy1.wav");
                    Thread.Sleep(500);
                    Program.GameConsole.SendCommand(@"play player/footsteps/new/land_tile_01.wav");
                    Thread.Sleep(200);
                    Program.GameConsole.SendCommand(@"play weapons/knife/knife_slash2.wav");
                    HasPlayedKnifeSoundThisRound = true;
                }

                base.AfterActivate();
            });

        }

        override public void Reset()
        {
            HasPlayedKnifeSoundThisRound = false;
            HasPlayedZeusSoundThisRound = false;
        }

    }
}
