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
    class de_train : Map
    {
        public override Int32 MapID { get; set; } = 1952408932;

        public de_train()
        {

            // Heaven
            TripWire heaven_ladder = new TripWire(
                new { 
                    x1 = 865, y1 = -394,
                    x2 = 863, y2 = -305,
                    x3 = 968, y3 = -312,
                    x4 = 963, y4 = -403,
                    z = 28
                }, 100, default, 100
            );
            heaven_ladder.resetOnLeave = true;
            heaven_ladder.OnTriggered += LadderDrop;
            TripWires.Add(heaven_ladder);
            // ---


            // PopLadder
            TripWire pop_ladder = new TripWire(
                new { 
                    x1 = -702, y1 = -340,
                    x2 = -700, y2 = -401,
                    x3 = -774, y3 = -402,
                    x4 = -780, y4 = -332,
                    z = 9
                }, 100, default, 100
            );
            pop_ladder.resetOnLeave = true;
            pop_ladder.OnTriggered += LadderDrop;
            TripWires.Add(pop_ladder);
            // ---

            // MindControl Connector
            TripWire mindcontrol_connector = new TripWire(
                new { 
                    x1 = 482, y1 = -524,
                    x2 = 479, y2 = -420,
                    x3 = 623, y3 = -416,
                    x4 = 623, y4 = -550,
                    z = 0
                }, 100, default
            );
            mindcontrol_connector.OnTriggered += MindControlDropWeapons;
            TripWires.Add(mindcontrol_connector);
            // ---

           // Leeeeroy
            TripWire leeroy_tripwire = new TripWire(
                new { 
                    x1 = 1168, y1 = 1446,
                    x2 = 1153, y2 = 1653,
                    x3 = 1265, y3 = 1653,
                    x4 = 1237, y4 = 1446,
                    z = 0
                }, 100, Team.Terrorists
            );
            leeroy_tripwire.OnTriggered += leeroy_punishment;
            TripWires.Add(leeroy_tripwire);
            // ---

           // stairs drop weapon
            TripWire stairs_tripwire = new TripWire(
                new { 
                    x1 = -969, y1 = -532,
                    x2 = -976, y2 = -670,
                    x3 = -1054, y3 = -673,
                    x4 = -1058, y4 = -526,
                    z = 0
                }, 100, Team.Terrorists, 100
            );
            stairs_tripwire.OnTriggered += stairs_drop;
            TripWires.Add(stairs_tripwire);
            // ---

        }

        public void LadderDrop(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "-forward; +jump; +duck;" });
            MindControlActions.Add(new MindControlAction { Sleep = 1000 });
            Punishment p = new MindControl(MindControlActions, false);
        }

        public void MindControlDropWeapons(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(340, -678, -22), AimLockDuration = 1000 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward; slot2;" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+jump" });
            MindControlActions.Add(new MindControlAction { Sleep = 300 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop; drop;" });
            Punishment p = new MindControl(MindControlActions, true);
        }

        public void leeroy_punishment(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(1438, 1463, 59), AimLockDuration = 250 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward; slot2;" });
            MindControlActions.Add(new MindControlAction { Sleep = 200 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop; drop;" });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(1306, 428, -136), AimLockDuration = 4000 });
            MindControlActions.Add(new MindControlAction { Sleep = 4000 });
            Punishment p = new MindControl(MindControlActions, true, 4000);
        }

        public void stairs_drop(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "-forward;" });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1006, -744, 185), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward; slot2;" });
            MindControlActions.Add(new MindControlAction { Sleep = 200 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+jump" });
            MindControlActions.Add(new MindControlAction { Sleep = 300 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop; drop;" });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-986, -1481, 0), AimLockDuration = 300 });
            Punishment p = new MindControl(MindControlActions, true);
        }

        

    }
}
