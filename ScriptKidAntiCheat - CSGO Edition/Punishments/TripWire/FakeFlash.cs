using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ScriptKidAntiCheat.Punishments
{
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
                // yeet
            }
            
        }

        private void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            // Make sure we change to weapon with a scope first (all pistols have it)
            Program.GameConsole.SendCommand("slot2");


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
