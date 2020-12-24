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
     PUNISHMENT: ToxicMate
     DESCRIPTION: Cause the cheater to say his or his teammates location on the map in text chat
    */
    class ToxicMate : Punishment
    {
        public Weapons LastActiveWeapon;
        public string CheaterLocation { get; set; }
        public string TeamLocation { get; set; }
        public override int ActivateOnRound { get; set; } = 3;
        public override bool DisposeOnReset { get; set; } = false;

        private static List<string> selfMsgs { get; set; }

        private static List<string> teamMsgs { get; set; }

        private static Random rng = new Random();

        public ToxicMate() : base(0, true, 10000) // 0 = Always active
        {
            selfMsgs = new List<string> { };
            selfMsgs.Add("I'm in #location# please kill me");
            selfMsgs.Add("I'm #location#!");
            selfMsgs.Add("I'm #location# now");
            teamMsgs = new List<string> { };
            teamMsgs.Add("My teammate is in #location#");
            teamMsgs.Add("He is #location#");
            teamMsgs.Add("Camping #location#");
            teamMsgs.Add("GOGOGO #location#");
        }

        override public void Tick(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (Program.GameData.MatchInfo.isFreezeTime) return;

                if (base.CanActivate() == false) return;

                if (Player.IsAlive())
                {
                    if(CheaterLocation != Player.Location.ToLower())
                    { 
                        CheaterLocation = Player.Location;
                        if (CheaterLocation.ToLower() != "" && !Helper.PlayerIsInSpawn())
                        {
                            CheaterLocation = CheaterLocation.Replace("BombsiteA", "Bombsite A");
                            CheaterLocation = CheaterLocation.Replace("BombsiteB", "Bombsite B");
                            int rnd = rng.Next(selfMsgs.Count);
                            ActivatePunishment(selfMsgs[rnd].Replace("#location#", CheaterLocation));
                        }
                    }
                } else
                {
                    // go through entities
                    foreach (var entity in GameData.Entities)
                    {

                        if (!entity.IsAlive() || entity.AddressBase == Player.AddressBase)
                        {
                            continue;
                        }

                        if (entity.Team == Player.Team)
                        {
                            TeamLocation = entity.Location;
                            if(entity.Location.ToLower() != TeamLocation.ToLower())
                            {
                                TeamLocation = CheaterLocation.Replace("BombsiteA", "Bombsite A");
                                TeamLocation = CheaterLocation.Replace("BombsiteB", "Bombsite B");
                                int rnd = rng.Next(teamMsgs.Count);
                                ActivatePunishment(teamMsgs[rnd].Replace("#location#", TeamLocation));
                            }

                        }

                        Thread.Sleep(10);
                        break;
                        
                    }
                }

                
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "ToxicMateException",
                    AnalyticsLabel = ex.Message
                });
            }

        }

        public void ActivatePunishment(string SayMsg)
        {
            if (base.CanActivate() == false) return;

            Task.Run(() =>
            {
                if(Program.Debug.ShowDebugMessages == false)
                {
                    Program.GameConsole.SendCommand("cl_chatfilters 0");
                    Program.GameConsole.SendCommand("echo cl_chatfilters");
                }

                Thread.Sleep(500);

                SayMsg = SayMsg.Replace("bombsite", "bombsite ");

                Program.GameConsole.SendCommand("Say " + SayMsg);

                base.AfterActivate();
            });
        }

        override public void Reset()
        {
            Program.GameConsole.SendCommand("cl_chatfilters 63");
        }

    }
}
