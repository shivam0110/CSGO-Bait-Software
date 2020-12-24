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
    class de_vertigo : Map
    {

        public de_vertigo()
        {

            // MindControl B Entry from CT
            TripWire mindcontrol = new TripWire(
                new { 
                    x1 = -1394, y1 = 540,
                    x2 = -1551, y2 = 540,
                    x3 = -1567, y3 = 943,
                    x4 = -1378, y4 = 980,
                    z = 0
                }, 50, Team.Terrorists
            );
            mindcontrol.OnTriggered += MindControl1;
            TripWires.Add(mindcontrol);
            // ---

            // MindControl B Entry from T
            TripWire mindcontrol2 = new TripWire(
                new { 
                    x1 = -2470, y1 = -7,
                    x2 = -2470, y2 = 108,
                    x3 = -2369, y3 = 114,
                    x4 = -2369, y4 = -20,
                    z = 0
                }, 50
            );
            mindcontrol2.OnTriggered += MindControl2;
            TripWires.Add(mindcontrol2);
            // ---

            // MindControl Ct Spawn
            TripWire mindcontrol3 = new TripWire(
                new { 
                    x1 = -1295, y1 = 995,
                    x2 = -1158, y2 = 1010,
                    x3 = -1155, y3 = 867,
                    x4 = -1310, y4 = 882,
                    z = 0
                }, 100, Team.CounterTerrorists
            );
            mindcontrol3.OnTriggered += MindControl3;
            TripWires.Add(mindcontrol3);
            // ---

            // MindControl A Entry from T
            TripWire mindcontrol4 = new TripWire(
                new { 
                    x1 = -1007, y1 = -1234,
                    x2 = -920, y2 = -1226,
                    x3 = -918, y3 = -1382,
                    x4 = -1029, y4 = -1387,
                    z = 0
                }, 50
            );
            mindcontrol4.OnTriggered += MindControl4;
            TripWires.Add(mindcontrol4);
            // ---

            // MindControl A Entry from CT
            TripWire yeeet = new TripWire(
                new { 
                    x1 = -86, y1 = -5,
                    x2 = -87, y2 = -58,
                    x3 = -164, y3 = -59,
                    x4 = -165, y4 = -4,
                    z = 0
                }, 50, Team.CounterTerrorists
            );
            yeeet.OnTriggered += Yeeeeeeeet;
            TripWires.Add(yeeet);
            // ---

        }

        public void MindControl1(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1557, 1081, 11821), AimLockDuration = 2500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 2500 });
            Punishment p = new MindControl(MindControlActions, true, 2500);
        }

        public void MindControl2(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-2767, 46, 11787), AimLockDuration = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+jump" });
            MindControlActions.Add(new MindControlAction { Sleep = 1000 });
            Punishment p = new MindControl(MindControlActions);
        }

        public void MindControl3(TripWire TripWire)
        {
            Weapons PrimaryWeapon = (Weapons)Program.GameData.Player.PrimaryWeapon;
            Weapons ActiveWeapon = (Weapons)Program.GameData.Player.ActiveWeapon;
            if (PrimaryWeapon != Weapons.Awp && ActiveWeapon != Weapons.Awp)
            {
                return;
            }

            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-1212, 1126, 11849), AimLockDuration = 1500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+jump; +duck;" });
            MindControlActions.Add(new MindControlAction { Sleep = 500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "-duck;" });
            MindControlActions.Add(new MindControlAction { Sleep = 1000 });
            Punishment p = new MindControl(MindControlActions, true);
        }

        public void MindControl4(TripWire TripWire)
        {
            List<MindControlAction> MindControlActions = new List<MindControlAction>();
            MindControlActions.Add(new MindControlAction { AimLockAtWorldPoint = new Vector3(-940, -1641, 11857), AimLockDuration = 1500 });
            MindControlActions.Add(new MindControlAction { ConsoleCommand = "+forward" });
            MindControlActions.Add(new MindControlAction { Sleep = 1500 });
            Punishment p = new MindControl(MindControlActions);
        }

        public void Yeeeeeeeet(TripWire TripWire)
        {
            // p = new Yeeeeeeeet(TripWire, new Vector3(-32, -31, 11857));
        }


    }
}
