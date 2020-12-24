using ScriptKidAntiCheat.Classes;
using ScriptKidAntiCheat.Utils;
using System;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using static ScriptKidAntiCheat.Utils.MouseHook;
using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Collections.Generic;
using ScriptKidAntiCheat.Classes.Utils;

namespace ScriptKidAntiCheat.Punishments
{
    /*
     PUNISHMENT: KnockKnockWhosThere
     DESCRIPTION: Unbind "e" key while near any doors and if they try to open the door play realistic knocking sound
    */
    class KnockKnockWhosThere : Punishment
    {
        public TripWire triggeringTripWire;

        public bool isPlaying = false;

        bool simulatedKeyDown = false;

        private IKeyboardMouseEvents m_GlobalHook;

        public KnockKnockWhosThere(TripWire TripWire) : base(0, false, 100) // 0 = Always active
        {
            triggeringTripWire = TripWire;
            Program.GameConsole.SendCommand("unbind e");
            // Keyboard events
            Program.m_GlobalHook.KeyDown += GlobalHookKeyDown;
        }
        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.E)
                {
                    ActivatePunishment();
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "KnockKnockWhosThereException1",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (!triggeringTripWire.IsBeingTripped)
                {
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "KnockKnockWhosThereException2",
                    AnalyticsLabel = ex.Message
                });
            }

        }
        public void ActivatePunishment()
        {
            if (base.CanActivate() == false) return;

            if (!isPlaying)
            {
                isPlaying = true;

                Task.Run(() => {

                    Program.GameConsole.SendCommand("play knock");

                    if (Helper.HasLocalSounds == false)
                    {
                        System.IO.Stream str = Properties.Resources.knock;
                        System.Media.SoundPlayer snd = new System.Media.SoundPlayer(str);
                        snd.Play();
                    }

                    Thread.Sleep(1000);
                    isPlaying = false;
                });

            }

            base.AfterActivate();
        }

        override public void Dispose()
        {
            Program.m_GlobalHook.KeyDown -= GlobalHookKeyDown;
            PlayerConfig.ResetConfig();
            base.Dispose();
        }

    }
}
