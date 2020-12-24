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
    class de_inferno : Map
    {
        public override Int32 MapID { get; set; } = 1767859556;

        public de_inferno()
        {

            // A Short / House
            TripWire mindcontrol = new TripWire(
                new { 
                    x1 = 941, y1 = 2221,
                    x2 = 935, y2 = 2190,
                    x3 = 894, y3 = 2190,
                    x4 = 894, y4 = 2221,
                    z = 0
                }, 100, Team.Terrorists
            );
            mindcontrol.OnTriggered += mindcontrolPunishment;
            TripWires.Add(mindcontrol);
            // ---

            // A Short / House
            TripWire a_short = new TripWire(
                new { 
                    x1 = 1928, y1 = 178,
                    x2 = 2016, y2 = 180,
                    x3 = 2024, y3 = -262,
                    x4 = 1934, y4 = -261,
                    z = 0
                }, 50, Team.Terrorists
            );
            a_short.OnTriggered += tripWirePunishments;
            TripWires.Add(a_short);
            // ---

            // A Apartments
            TripWire a_aps = new TripWire(
                new { 
                    x1 = 1796, y1 = -251,
                    x2 = 1793, y2 = -395,
                    x3 = 1708, y3 = -383,
                    x4 = 1709, y4 = -246,
                    z = 0
                }, 50, default, 50
            );
            a_aps.checkFromMemory = true;
            a_aps.OnTriggered += DropThatGun;
            TripWires.Add(a_aps);
            // ---

            // B From Banana
            TripWire b_banana = new TripWire(
                new { 
                    x1 = 661, y1 = 2292,
                    x2 = 910, y2 = 2320,
                    x3 = 908, y3 = 2436,
                    x4 = 653, y4 = 2428,
                    z = 0
                }, 50, Team.Terrorists, 50
            );
            b_banana.OnTriggered += tripWirePunishments;
            TripWires.Add(b_banana);
            // ---

        }

        public void DropThatGun(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(1750, -447, 363), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { Sleep = 100 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop; drop;" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            
            Vector3 AimDirection = Program.GameData.Player.AimDirection;
            if(AimDirection.X < 0)
            {
                // Coming from T side (look back in correct direction after throwing weapons)
                MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(1429, -321, 320), AimLockDuration = 500 });
            }
            else
            {
                // Coming from CT side (look back in correct direction after throwing weapons)
                MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(1910, -325, 320), AimLockDuration = 500 });
            }

            Punishment p = new MindControl(MindControlActions);
        }

        public void mindcontrolPunishment(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "drop; drop;" });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(691, 2220, 201), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward; drop; drop;" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(767, 2981, 198), AimLockDuration = 3000 });
            MindControlActions.Add(new MindControlAction { Sleep = 3000 });
            Punishment p = new MindControl(MindControlActions, true, 3000);
        }


    }
}
