using System;
using System.Linq;
using System.Text.RegularExpressions;
using ScriptKidAntiCheat.Classes;
using ScriptKidAntiCheat.Internal;
using ScriptKidAntiCheat.Utils;

namespace ScriptKidAntiCheat.Data
{
    public class GameData : U_ThreadedComponent
    {
        protected override string ThreadName => nameof(GameData);

        private GameProcess GameProcess { get; set; }

        public Player Player { get; set; }

        public MatchInfo MatchInfo { get; set; }

        public int ClientState { get; private set; } = 0;

        public Entity[] Entities { get; private set; }

        public int PlayerID { get; private set; }

        public GameData(GameProcess gameProcess)
        {
            GameProcess = gameProcess;
            Player = new Player();
            MatchInfo = new MatchInfo();
            Entities = Enumerable.Range(0, 1024).Select(index => new Entity(index)).ToArray();
            Program.GameConsole.ConsoleRead += NewConsoleOuput;
        }

        public void NewConsoleOuput(object sender, ConsoleReadEventArgs e)
        {
            string output = e.Response;

            // Check if response contains a steamid
            var regex = new Regex(@"(?<=STEAM_)([^\s]*)");
            if (regex.IsMatch(output))
            {
                Console.WriteLine("STATUS: STEAM_" + regex.Match(output).Groups[1].Value);
                Log.AddPlayer("STEAM_" + regex.Match(output).Groups[1].Value);
                Program.GameData.MatchInfo.AddSteamId("STEAM_" + regex.Match(output).Groups[1].Value);
            }

            // Read NickName from Console
            var nameRegex = new Regex(@"(?<=name\"" = \"")(.*)(?=\"" \()");
            if (nameRegex.IsMatch(output))
            {
                Console.WriteLine("NickName From Console");
                Player.NickName = nameRegex.Match(output).ToString();
            }

            // Read Map name from Console as fallback
            var mapRegex = new Regex(@"(?<=Map\: )(.*)");
            var mapRegex2 = new Regex(@"(?<=map     \: )(.*)");
            if (mapRegex.IsMatch(output))
            {
                MatchInfo.ConsoleMapName = mapRegex.Match(output).ToString();
            }
            else if(mapRegex2.IsMatch(output))
            {
                MatchInfo.ConsoleMapName = mapRegex2.Match(output).ToString();
            }

            // Read MatchID from Console as fallback
            var matchIdRegex = new Regex(@"(?<=Connected to \=)(.*)");
            if (matchIdRegex.IsMatch(output))
            {
                MatchInfo.ConsoleMatchID = matchIdRegex.Match(output).ToString();
            }

            // Read if game is wingman or not from console
            if (output.Contains("players :") && output.Contains("/4 max)"))
            {
                MatchInfo.IsWingman = true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            MatchInfo = default;
            Entities = default;
            Player = default;
            GameProcess = default;
        }

        protected override void FrameAction()
        {
            if (!GameProcess.IsValidAndActiveWindow)
            {
                return;
            }

            int AliveTeammates = 0;

            IntPtr dwClientState = GameProcess.ModuleEngine.Read<IntPtr>(Offsets.dwClientState);
            if (dwClientState != IntPtr.Zero)
            {
                ClientState = GameProcess.Process.Read<int>(dwClientState + Offsets.dwClientState_State);
            }

            // Update match data
            MatchInfo.Update(GameProcess);

            // Update player data
            Player.Update(GameProcess);

            foreach (var entity in Entities)
            {
                // Update game entities data
                entity.Update(GameProcess);

                // Fallback try read nick from memory if fails from console
                if(Player.NickName == "" && entity.AddressBase == Player.AddressBase && Player.NickName != entity.NickName)
                {
                    Console.WriteLine("NickName From Memory");
                    Player.NickName = entity.NickName;
                }

                if (entity.IsAlive() && entity.AddressBase != Player.AddressBase)
                {
                    if (entity.Team == Player.Team)
                    {
                        AliveTeammates++;
                    }
                }


            }

            MatchInfo.AliveTeammates = AliveTeammates;
        }
    }
}
