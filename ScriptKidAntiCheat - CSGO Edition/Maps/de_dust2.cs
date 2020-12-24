using ScriptKidAntiCheat.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Punishments;
using ScriptKidAntiCheat.Internal;
using ScriptKidAntiCheat.Data;
using System.Threading;
using SharpDX;

namespace ScriptKidAntiCheat
{
    class de_dust2 : Map
    {
        public override Int32 MapID { get; set; } = 1683973476;

        public de_dust2()
        {

            // CT SPAWN MID
            TripWire ct_mid = new TripWire(
                new { 
                    x1 = -346, y1 = 2001,
                    x2 = -349, y2 = 2303,
                    x3 = -519, y3 = 2302,
                    x4 = -517, y4 = 1965,
                    z = 0
                }, 50, Team.CounterTerrorists, 50
            );
            ct_mid.OnTriggered += RushMid;
            TripWires.Add(ct_mid);
            // ---

            // B Window
            TripWire b_window = new TripWire(
                new { 
                    x1 = -1410, y1 = 2629,
                    x2 = -1408, y2 = 2726,
                    x3 = -1362, y3 = 2721,
                    x4 = -1366, y4 = 2627,
                    z = 0
                }, 50, default, 50
            );
            b_window.OnTriggered += ByeByeGuns;
            TripWires.Add(b_window);
            // ---

            // A LONG
            TripWire a_long = new TripWire(
                new { 
                    x1 = 1222, y1 = 1998,
                    x2 = 1692, y2 = 1756,
                    x3 = 1845, y3 = 2139,
                    x4 = 1212, y4 = 2096,
                    z = 0
                }, 50, Team.Terrorists
            );
            a_long.OnTriggered += tripWirePunishments;
            TripWires.Add(a_long);
            // ---

            // A CW
            TripWire a_cw = new TripWire(
                new { 
                    x1 = 552, y1 = 1811,
                    x2 = 231, y2 = 1849,
                    x3 = 196, y3 = 2041,
                    x4 = 554, y4 = 2031,
                    z = 0
                }, 50, Team.Terrorists
            );
            a_cw.OnTriggered += tripWirePunishments;
            TripWires.Add(a_cw);
            // ---

            // B Main
            TripWire b_main = new TripWire(
                new { 
                    x1 = -1828, y1 = 1798,
                    x2 = -2167, y2 = 1788,
                    x3 = -2163, y3 = 1880,
                    x4 = -1830, y4 = 1874,
                    z = 0
                }, 50, Team.Terrorists
            );
            b_main.OnTriggered += tripWirePunishments;
            TripWires.Add(b_main);
            // ---

            // B Door
            TripWire b_door = new TripWire(
                new { 
                    x1 = -1376, y1 = 2257,
                    x2 = -1299, y2 = 2310,
                    x3 = -1278, y3 = 2063,
                    x4 = -1362, y4 = 2091,
                    z = 0
                }, 50, Team.Terrorists
            );
            b_door.OnTriggered += tripWirePunishments;
            TripWires.Add(b_door);
            // ---

        }

        public void RushMid(TripWire TripWire)
        {
            if (Program.GameData.MatchInfo.RoundNumber < 3 && !Program.Debug.IgnoreActivateOnRound) return; 
            Program.GameConsole.SendCommand("-forward");
        }

        public void ByeByeGuns(TripWire TripWire)
        {
            if (Program.GameData.MatchInfo.RoundNumber < 3 && !Program.Debug.IgnoreActivateOnRound) return;

            List<MindControlAction> MindControlActions = new List<MindControlAction>();

            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1444, 2982, 334), AimLockDuration = 1100 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 300 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop" });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            Punishment p = new MindControl(MindControlActions, true);
        }

    }
}
