using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: FakeFlash
     DESCRIPTION: Simulate flashbang by changing crosshair settings to cover the whole screen and fade from 0 to 100% white
    */
    class FakeFlash : Punishment
    {
        private int FlashDuration { get; set; }

        public FakeFlash(int FakeFlashDuration = 2000) : base(0) // 0 = Always active
        {
            FlashDuration = FakeFlashDuration;
            ActivatePunishment();
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
              
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "FakeFlashException",
                    AnalyticsLabel = ex.Message
                });
            }
            
        }

        private void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            Weapons ActiveWeapon = (Weapons)Player.ActiveWeapon;

            // Make sure we change to weapon with a scope first (all pistols have it)
            if (ActiveWeapon != Weapons.Knife_CT && 
                ActiveWeapon != Weapons.Knife_T && 
                ActiveWeapon != Weapons.Smoke &&
                ActiveWeapon != Weapons.Grenade &&
                ActiveWeapon != Weapons.Molotov &&
                ActiveWeapon != Weapons.Incendiary &&
                ActiveWeapon != Weapons.Flashbang)
            {
                Program.GameConsole.SendCommand("slot2");
            }

            Program.GameConsole.SendCommand("play flashed");

            if(Helper.HasLocalSounds == false)
            {
                System.IO.Stream str = Properties.Resources.flashed;
                System.Media.SoundPlayer snd = new System.Media.SoundPlayer(str);
                snd.Play();
            }

            Program.GameConsole.SendCommand("cl_crosshairthickness 999; cl_crosshairsize 999; cl_crosshaircolor 5; cl_crosshairdot 1; cl_crosshaircolor_r 255; cl_crosshaircolor_g 255; cl_crosshaircolor_b 255; cl_crosshairalpha 0;");

            int alpha = 5;
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(15);
                alpha += 25;
                Program.GameConsole.SendCommand("cl_crosshairalpha " + alpha + ";");
            }

            Task.Run(() =>
            {
                Thread.Sleep(FlashDuration);
                alpha -= 5;
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(15);
                    alpha -= 25;
                    Program.GameConsole.SendCommand("cl_crosshairalpha " + alpha + ";");
                }
                PlayerConfig.ResetConfig();
            });

            //base.AfterActivate();
        }

    }
}
