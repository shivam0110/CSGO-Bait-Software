using System;
using System.Collections.Generic;
using SharpDX;
using ScriptKidAntiCheat.Punishments;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Internal;
using ScriptKidAntiCheat.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ScriptKidAntiCheat.Win32;
using ScriptKidAntiCheat.Classes.Utils;

namespace ScriptKidAntiCheat.Classes
{
    public abstract class Map : IDisposable
    {
        bool disposed = false;
        public virtual Int32 MapID { get; set; }

        public List<Punishment> Punishments = new List<Punishment>();

        public List<TripWire> TripWires = new List<TripWire>();

        protected Map()
        {
            // Setting up Punishments
            Punishments.Add(new NadeKing());
            Punishments.Add(new ToxicMate());
            Punishments.Add(new HotPotato());
            Punishments.Add(new JumpingJack());
            Punishments.Add(new NoPlantOrDefuse());
            Punishments.Add(new NoSilentWalk());
            Punishments.Add(new MaxRecoil());
            Punishments.Add(new HeavyKnife());
            Punishments.Add(new LuckyLuke());
            Punishments.Add(new BurningMan());

            // ### Disable punishments (older stuff)
            // # Punishments.Add(new FlashInYourFace());
            // # Punishments.Add(new InvertMouseAds());
            // # Punishments.Add(new NoSpray4U());
            // # Punishments.Add(new BigSpender());
            // ###

            Program.GameData.MatchInfo.OnMatchNewRound += NewRound;
        }

        public void resetTripWires()
        {
            Console.WriteLine("Reset traps");
            foreach (TripWire tripwire in TripWires)
            {
                tripwire.reset();
            }
        }

        virtual public void NewRound(object sender, EventArgs e)
        {
            Program.GameConsole.SendCommand("status");
            resetTripWires();
            PlayerConfig.ResetConfig();
        }

        public void Dispose()
        {
            foreach (Punishment Punishment in Punishments)
            {
                Punishment.Dispose();
            }
            Program.GameData.MatchInfo.OnMatchNewRound -= NewRound;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Console.WriteLine("disposed");
            }

            disposed = true;
        }

        public enum stage1TripWirePunishments { }
        public enum stage2TripWirePunishments { ViolenceSpeedMomentum, ReverseSpeedhack, DrunkenMaster }

        public void tripWirePunishments(TripWire TripWire)
        {
            string punishment = "";

            if (Program.GameData.MatchInfo.RoundNumber >= 3 || Program.Debug.TripWireStage == 2)
            {
                stage2TripWirePunishments ps2 = (stage2TripWirePunishments)(new Random()).Next(0, 3);
                punishment = ps2.ToString();
            }
            else if (Program.GameData.MatchInfo.RoundNumber < 3 || Program.Debug.TripWireStage == 1)
            {
                return;
                //stage1TripWirePunishments ps1 = (stage1TripWirePunishments)(new Random()).Next(0, 3);
                //punishment = ps1.ToString();
            }

            Activator.CreateInstance(Type.GetType("ScriptKidAntiCheat.Punishments." + punishment));

            Thread.Sleep(500);

            if (Program.Debug.ShowDebugMessages)
            {
                Program.GameConsole.SendCommand("Say \"TripWire Triggered (" + punishment + ")\"");
            }

        }

    }
}
