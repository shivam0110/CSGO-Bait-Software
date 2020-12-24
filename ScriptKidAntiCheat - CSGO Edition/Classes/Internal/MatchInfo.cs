using ScriptKidAntiCheat.Data;
using ScriptKidAntiCheat.Internal.Raw;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Utils.Maths;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Internal
{
    public class MatchInfo
    {
        public IntPtr AddressBase { get; protected set; }
        public IntPtr GameRulesProxy { get; protected set; }
        public GlobalVars GlobalVars { get; protected set; }
        public RoundResults RoundResults { get; protected set; }
        public string MapName { get; protected set; } = "";
        public string ConsoleMapName { get; set; } = "";
        public string MatchID { get; set; }
        private string PrevMatchID { get; set; }
        public string ConsoleMatchID { get; set; }

        public bool isMatchPoint = false;

        public bool IsWingman = false;

        public bool isFreezeTime = false;

        public bool isWarmup = false;

        public bool IsMatchmaking = false;

        public bool isBombPlanted = false;

        public string SpawnLocationName = "";

        public int T_Score = 0;

        public int CT_Score = 0;
        public int AliveTeammates { get; set; } = 0;

        public int LastFreezeTimeRound = -1;

        public bool OnMatchNewRoundFirstTime = true;

        public int RoundNumber = 0;

        public string RoundTime = "";

        public int SecondsRemainingOfRound = 0;
        public List<string> SteamIds { get; set; }

        public event EventHandler OnMatchNewRound;

        protected IntPtr ReadAddressBase(GameProcess gameProcess)
        {
            return gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwLocalPlayer);
        }
        protected IntPtr ReadGameRulesProxy(GameProcess gameProcess)
        {
            return gameProcess.ModuleClient.Read<IntPtr>(Offsets.dwGameRulesProxy);
        }
        protected string ReadMapName(GameProcess gameProcess)
        {
            IntPtr dwClientState = gameProcess.ModuleEngine.Read<IntPtr>(Offsets.dwClientState);
            
            if (dwClientState != IntPtr.Zero)
            {
                string MapName = MemoryRead.ReadString(gameProcess.ModuleClient, dwClientState, Offsets.dwClientState_Map, 32);
                if(MapName != "")
                {
                    return MapName;
                }
            }

            if(ConsoleMapName != "")
            {
                return ConsoleMapName; // Fallback
            }

            return "";
        }

        protected GlobalVars ReadGlobalVars(GameProcess gameProcess)
        {
            return gameProcess.ModuleEngine.Read<GlobalVars>(Offsets.dwGlobalVars);
        }

        public void AddSteamId(string SteamID)
        {
            if(SteamIds == null)
            {
                SteamIds = new List<string> { };
            }

            if(SteamIds.Count > 0)
            {
                foreach (string ID in SteamIds)
                {
                    if (ID == SteamID)
                    {
                        return;
                    }
                }
            }

            SteamIds.Add(SteamID);
        }

        public bool checkFreezetime(GameProcess gameProcess)
        {
            if (GameRulesProxy != IntPtr.Zero)
            {
                byte isFreezeTime = gameProcess.Process.Read<byte>(GameRulesProxy + Offsets.m_bFreezePeriod);
                if(isFreezeTime == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public bool checkWarmup(GameProcess gameProcess)
        {
            if (GameRulesProxy != IntPtr.Zero)
            {
                bool isWarmup = gameProcess.Process.Read<bool>(GameRulesProxy + Offsets.m_bWarmupPeriod);
                return isWarmup;
            }

            return false;
        }

        public bool checkGameType(GameProcess gameProcess)
        {
            if (GameRulesProxy != IntPtr.Zero)
            {
                byte m_bIsQueuedMatchmaking = gameProcess.Process.Read<byte>(GameRulesProxy + Offsets.m_bIsQueuedMatchmaking);
                if (m_bIsQueuedMatchmaking == 1 && MatchID != null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool checkBombPlanted(GameProcess gameProcess)
        {
            if (GameRulesProxy != IntPtr.Zero)
            {
                byte isBombPlanted = gameProcess.Process.Read<byte>(GameRulesProxy + Offsets.m_bBombPlanted);
                if (isBombPlanted == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetCurrentMatchRound(GameProcess gameProcess)
        {
            if(GameRulesProxy != IntPtr.Zero)
            {
                int m_totalRoundsPlayed = gameProcess.Process.Read<int>(GameRulesProxy + Offsets.m_totalRoundsPlayed);

                return m_totalRoundsPlayed+1;
            }

            return 0;
        }

        public string GetCurrentRoundTime(GameProcess gameProcess)
        {
            if (GameRulesProxy != IntPtr.Zero)
            {
                float m_fRoundStartTime = gameProcess.Process.Read<float>(GameRulesProxy + Offsets.m_fRoundStartTime);
                int m_IRoundTime = gameProcess.Process.Read<int>(GameRulesProxy + Offsets.m_IRoundTime);
                int RoundRemainingTimeTotal = (int)(m_fRoundStartTime + m_IRoundTime) - (int)GlobalVars.curtime;
                SecondsRemainingOfRound = RoundRemainingTimeTotal;
                int RoundRemainingTimeMinutes = RoundRemainingTimeTotal / 60;
                int RoundRemainingTimeSeconds = RoundRemainingTimeTotal - (RoundRemainingTimeMinutes*60);
                if(RoundRemainingTimeSeconds < 0 || RoundRemainingTimeSeconds < 0)
                {
                    return "00:00";
                } else
                {
                    return $"{RoundRemainingTimeMinutes:D2}" + ":" + $"{RoundRemainingTimeSeconds:D2}";
                }
                
            }

            return "";
        }

        public RoundResults GetRoundResults(GameProcess gameProcess)
        {
            if (GameRulesProxy != IntPtr.Zero)
            {
                RoundResults m_iMatchStats_RoundResults = gameProcess.Process.Read<RoundResults>(GameRulesProxy + 2480);
   
                return m_iMatchStats_RoundResults;
            }

            return default;
        }

        public string GetMatchID(GameProcess gameProcess)
        {
            long MemMatchID = gameProcess.ModuleClient.Read<long>(Offsets.m_iLastCompID);

            if(MemMatchID != 0)
            {
                return MemMatchID.ToString();
            } 
            else if(ConsoleMatchID != "")
            {
                return ConsoleMatchID;
            }

            return null;
        }

        public bool Update(GameProcess gameProcess)
        {
            AddressBase     = ReadAddressBase(gameProcess);
            MapName         = ReadMapName(gameProcess);
            GameRulesProxy  = ReadGameRulesProxy(gameProcess);
            GlobalVars      = ReadGlobalVars(gameProcess);
            isFreezeTime    = checkFreezetime(gameProcess);
            isWarmup        = checkWarmup(gameProcess);
            MatchID         = GetMatchID(gameProcess);
            IsMatchmaking   = checkGameType(gameProcess);
            isBombPlanted   = checkBombPlanted(gameProcess);
            RoundNumber     = GetCurrentMatchRound(gameProcess);
            RoundTime       = GetCurrentRoundTime(gameProcess);
            RoundResults    = GetRoundResults(gameProcess);

            if(MatchID != PrevMatchID && MatchID.Length == 19)
            {
                IsWingman = false;
                PrevMatchID = MatchID;
                Console.WriteLine("New MatchID Detected: " + MatchID);
            }

            if (isFreezeTime == true && isWarmup == false && Program.GameData.Player.Location != "" && Program.GameData.Player.Location != null)
            {
                SpawnLocationName = Program.GameData.Player.Location;
            }

            // Get Team Score
            int score_terrorists = 0;
            int score_counterterrorists = 0;
            foreach(var field in typeof(RoundResults).GetFields())
            {
                int RoundResult = (int)field.GetValue(RoundResults);

                /*
                    0 = Round was not played yet
                    // CT Win
                    1 = CT win through elimination
                    2 = CT win through rescuing the hostage
                    3 = CT win through defusing the bomb
                    4 = CT win because the time ran out
                    // T Win
                    5 = T win through elimination
                    6 = T win through bomb explosion
                    7 = T win because the time ran out
                */

                if (RoundResult == 0) continue;

                if (RoundResult > 4)
                {
                    score_terrorists++;
                } else
                {
                    score_counterterrorists++;
                }
            }

            T_Score = score_terrorists;
            CT_Score = score_counterterrorists;

            if (T_Score == 15 || CT_Score == 15)
            {
                isMatchPoint = true;
            } else
            {
                isMatchPoint = false;
            }

           // Console.WriteLine("{0}, {1}", T_Score, CT_Score);

            if (RoundNumber != LastFreezeTimeRound && isFreezeTime)
            {
                Program.GameData.Player.PrimaryWeapon = 0;
                LastFreezeTimeRound = RoundNumber; // Only trigger event once per round (during freezetime)
                Task.Run(() =>
                {
                    if (OnMatchNewRoundFirstTime == false)
                    {
                        EventHandler handler = OnMatchNewRound;
                        handler?.Invoke(this, null);
                    }
                    OnMatchNewRoundFirstTime = false;
                });
            }

            return true;
        }

    }
}
