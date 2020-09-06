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

        }

        public void LadderDrop(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "-forward; +jump; +duck;" });
            MindControlActions.Add(new MindControlAction { Sleep = 1000 });
            Punishment p = new MindControl(MindControlActions, false);
        }
    }
}
