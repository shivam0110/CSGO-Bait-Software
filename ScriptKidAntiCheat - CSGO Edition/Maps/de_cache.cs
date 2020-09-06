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
    class de_cache : Map
    {
        public override Int32 MapID { get; set; } = 1667196260;

        public de_cache()
        {

            // B
            TripWire b = new TripWire(
                new { 
                    x1 = 226, y1 = -886,
                    x2 = 220, y2 = -448,
                    x3 = -426, y3 = -458,
                    x4 = -428, y4 = -1034,
                    z = 0
                }, 50, Team.Terrorists
            );
            b.OnTriggered += tripWirePunishments;
            TripWires.Add(b);
            // ---

            // A main
            TripWire a_main = new TripWire(
                new { 
                    x1 = 483, y1 = 1598,
                    x2 = 830, y2 = 1639,
                    x3 = 851, y3 = 1832,
                    x4 = 504, y4 = 1818,
                    z = 0
                }, 50, Team.Terrorists
            );
            a_main.OnTriggered += tripWirePunishments;
            TripWires.Add(a_main);
            // ---

            // A main
            TripWire a_cw = new TripWire(
                new { 
                    x1 = 453, y1 = 1860,
                    x2 = 463, y2 = 1405,
                    x3 = 283, y3 = 1414,
                    x4 = 297, y4 = 1872,
                    z = 0
                }, 50, Team.Terrorists
            );
            a_cw.OnTriggered += tripWirePunishments;
            TripWires.Add(a_cw);
            // ---

            // A door
            TripWire a_door = new TripWire(
                new { 
                    x1 = 352, y1 = 1957,
                    x2 = 131, y2 = 1936,
                    x3 = 141, y3 = 2224,
                    x4 = 365, y4 = 2230,
                    z = 0
                }, 100, default
            );
            a_door.resetOnLeave = true;
            a_door.OnTriggered += KnockKnocWhosThere;
            TripWires.Add(a_door);
            // ---

            // Mid Boost
            TripWire mid_boost = new TripWire(
                new { 
                    x1 = 1001, y1 = 495,
                    x2 = 838, y2 = 490,
                    x3 = 866, y3 = 602,
                    x4 = 996, y4 = 596,
                    z = 1743
                }, 100, default, 25
            );
            mid_boost.checkFromMemory = true;
            mid_boost.OnTriggered += MindControl1;
            TripWires.Add(mid_boost);
            // ---

        }

        public void KnockKnocWhosThere(TripWire TripWire)
        {
            Punishment p = new KnockKnockWhosThere(TripWire);
        }

        public void MindControl1(TripWire TripWire)
        {

            Weapons ActiveWeapon = (Weapons)Program.GameData.Player.ActiveWeapon;

            List<MindControlAction> MindControlActions = new List<MindControlAction>();

            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(900, 545, 1978), AimLockDuration = 1000 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "-forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 200 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+jump" });
            MindControlActions.Add(new MindControlAction { Sleep = 200 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop" });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(735, 608, 1884), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(436, 212, 1807), AimLockDuration = 1000 });
            MindControlActions.Add(new MindControlAction { Sleep = 1000 });

            Punishment p = new MindControl(MindControlActions, true, 2600);
        }

    }
}
