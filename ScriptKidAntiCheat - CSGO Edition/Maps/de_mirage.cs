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
    class de_mirage : Map
    {
        public override Int32 MapID { get; set; } = 1834968420;

        public de_mirage()
        {

            // T Spawn (debugging)
            if(Program.Debug.ShowDebugMessages)
            {
                TripWire t_spawn = new TripWire(
                    new { 
                        x1 = 1032, y1 = 351,
                        x2 = 957, y2 = 431,
                        x3 = 964, y3 = 673,
                        x4 = 1135, y4 = 646,
                        z = 0
                    }, 100, Team.Terrorists
                );
                t_spawn.resetOnLeave = false;
                t_spawn.OnTriggered += debugging_tripwire;
                TripWires.Add(t_spawn);
                // ---
            }

            // B Entrance
            TripWire b_entrance = new TripWire(
                new { 
                    x1 = -1952, y1 = 675,
                    x2 = -2093, y2 = 671,
                    x3 = -2089, y3 = 831,
                    x4 = -1952, y4 = 849,
                    z = 0
                }, 50, Team.Terrorists
            );
            b_entrance.OnTriggered += tripWirePunishments;
            TripWires.Add(b_entrance);
            // ---

            // B Window
            TripWire b_window = new TripWire(
                new { 
                    x1 = -1723, y1 = 629,
                    x2 = -1722, y2 = 523,
                    x3 = -1858, y3 = 534,
                    x4 = -1865, y4 = 629,
                    z = 24
                }, 50, Team.Terrorists
            );
            b_window.OnTriggered += tripWirePunishments;
            TripWires.Add(b_window);
            // ---

            // Connector to MID
            TripWire connector = new TripWire(
                new { 
                    x1 = -824, y1 = -843,
                    x2 = -534, y2 = -846,
                    x3 = -505, y3 = -1268,
                    x4 = -833, y4 = -1251,
                    z = 0
                }, 50, Team.Terrorists
            );
            connector.OnTriggered += tripWirePunishments;
            TripWires.Add(connector);
            // ---

            // CW to B
            TripWire cw = new TripWire(
                new { 
                    x1 = -676, y1 = -241,
                    x2 = -945, y2 = -295,
                    x3 = -968, y3 = -21,
                    x4 = -704, y4 = -22,
                    z = 0
                }, 50, Team.Terrorists
            );
            cw.OnTriggered += tripWirePunishments;
            TripWires.Add(cw);
            // ---

            // A main
            TripWire a_main = new TripWire(
                new
                {
                    x1 = 50, y1 = -1358,
                    x2 = 38, y2 = -1650,
                    x3 = -138, y3 = -1653,
                    x4 = -165, y4 = -1365,
                    z = -93
                }, 50, Team.Terrorists
            );
            a_main.OnTriggered += tripWirePunishments;
            TripWires.Add(a_main);
            // ---

            // Mid Window
            TripWire black_magic_window = new TripWire(
                new
                {
                    x1 = -1101, y1 = -536,
                    x2 = -1100, y2 = -718,
                    x3 = -1050, y3 = -715,
                    x4 = -1052, y4 = -524,
                    z = -143
                }, 50, default, 25
            );
            black_magic_window.checkFromMemory = true;
            black_magic_window.OnTriggered += DropWeaponsBehindMe;
            TripWires.Add(black_magic_window);
            // ---

            // A Palace Entrance
            TripWire a_palace = new TripWire(
                new
                {
                    x1 = -21, y1 = -2166,
                    x2 = -18, y2 = -2020,
                    x3 = 150, y3 = -2031,
                    x4 = 87, y4 = -2185,
                    z = 0
                }, 50, Team.Terrorists
            );
            a_palace.OnTriggered += tripWirePunishments;
            TripWires.Add(a_palace);
            // ---

            // Window B House
            /*
            TripWire window_b_house = new TripWire(
                new
                {
                    x1 = -1187, y1 = 791,
                    x2 = -1186, y2 = 664,
                    x3 = -1294, y3 = 664,
                    x4 = -1290, y4 = 807,
                    z = 0
                }, 100, default
            );
            window_b_house.OnTriggered += ByeByeGuns;
            TripWires.Add(window_b_house);
            // ---
            */

        }

        public void DropWeaponsBehindMe(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1255, -623, -100), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop; drop;" });
            MindControlActions.Add(new MindControlAction { Sleep = 50 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+back;" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            Punishment p = new MindControl(MindControlActions);
        }

        public void debugging_tripwire(TripWire TripWire)
        {
            //Punishment p = new ReverseSpeedhack();
            Punishment p = new DrunkenMaster();
        }

        public void ByeByeGuns(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1233, 869, -39), AimLockDuration = 1500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 1500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop;" });
            MindControlActions.Add(new MindControlAction { Sleep = 125 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop;" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1447, 735, 16), AimLockDuration = 100 });
            Punishment p = new MindControl(MindControlActions, true, 2500);
        }


    }
}
