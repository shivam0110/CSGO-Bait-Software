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
    class de_nuke : Map
    {
        public override Int32 MapID { get; set; } = 1851745636;

        public de_nuke()
        {

            // A House Entrance
            TripWire a_house = new TripWire(
                new { 
                    x1 = 320, y1 = -887,
                    x2 = 321, y2 = -796,
                    x3 = 478, y3 = -788,
                    x4 = 477, y4 = -891,
                    z = -351
                }, 50, Team.Terrorists
            );
            a_house.OnTriggered += tripWirePunishments;
            TripWires.Add(a_house);
            // ---


            // A Main
            TripWire a_main = new TripWire(
                new { 
                    x1 = 765, y1 = -1467,
                    x2 = 391, y2 = -1482,
                    x3 = 391, y3 = -1207,
                    x4 = 799, y4 = -1208,
                    z = -351
                }, 50, Team.Terrorists
            );
            a_main.OnTriggered += tripWirePunishments;
            TripWires.Add(a_main);
            // ---

            // Small Door A & B
            TripWire small_doors = new TripWire(
                new { 
                    x1 = 520, y1 = -1386,
                    x2 = 12, y2 = -1388,
                    x3 = -2, y3 = -1165,
                    x4 = 498, y4 = -1151,
                    z = 0
                }, 100, default
            );
            small_doors.resetOnLeave = true;
            small_doors.OnTriggered += KnockKnocWhosThere;
            TripWires.Add(small_doors);
            // ---

            // Big doors B
            TripWire big_door = new TripWire(
                new { 
                    x1 = 1268, y1 = -1207,
                    x2 = 846, y2 = -1209,
                    x3 = 825, y3 = -790,
                    x4 = 1251, y4 = -760,
                    z = 0
                }, 100, default
            );
            big_door.resetOnLeave = true;
            big_door.OnTriggered += KnockKnocWhosThere;
            TripWires.Add(big_door);
            // ---

            // Silo
            TripWire silo = new TripWire(
                new { 
                    x1 = 238, y1 = -1490,
                    x2 = 242, y2 = -1782,
                    x3 = -98, y3 = -1765,
                    x4 = -10, y4 = -1471,
                    z = -80
                }, 100, default
            );
            silo.OnTriggered += JumpToDeath;
            TripWires.Add(silo);
            // ---

            // Ct ladder
            TripWire ct_ladder = new TripWire(
                new { 
                    x1 = 1106, y1 = -467,
                    x2 = 1104, y2 = -411,
                    x3 = 1187, y3 = -410,
                    x4 = 1185, y4 = -480,
                    z = -175
                }, 100, default
            );
            ct_ladder.resetOnLeave = true;
            ct_ladder.OnTriggered += denyLadderClimb;
            TripWires.Add(ct_ladder);
            // ---

        }

        public void KnockKnocWhosThere(TripWire TripWire)
        {
            Punishment p = new KnockKnockWhosThere(TripWire);
        }

        public void JumpToDeath(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(239, -1658, 25), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(412, -1865, 79), AimLockDuration = 1000 });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+jump;" });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+duck;" });
            MindControlActions.Add(new MindControlAction { Sleep = 2000 });
            Punishment p = new MindControl(MindControlActions, true, 3000);
        }

        public void denyLadderClimb(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "-forward; +jump; +duck;" });
            MindControlActions.Add(new MindControlAction { Sleep = 1000 });
            Punishment p = new MindControl(MindControlActions, false);
        }


    }
}
