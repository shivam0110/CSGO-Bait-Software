using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace ScriptKidAntiCheat.Utils
{
    /*
     * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
     */
    public static class Offsets
    {
        // My own offset repo
        public const string OffsetsURL = "https://gitlab.com/sheetergang/csgo-memory-offsets/-/raw/master/offsets.txt";
        public static string LastUpdatedOffsets = "";

        // Fallback use hazerdumper repo if date is newer
        public const string OffsetsURLHazerDumper = "https://raw.githubusercontent.com/frk1/hazedumper/master/csgo.cs";

        public static int dwClientState;
        public static int dwClientState_Map;
        public static int dwClientState_ViewAngles;
        public static int dwClientState_State;
        public static int dwLocalPlayer;
        public static int dwEntityList;
        public static int dwGameRulesProxy;
        public static int dwGlobalVars;
        public static int dwMouseEnable;
        public static int dwRadarBase;
        public static int m_iradarBasePtr;
        public static int m_iradarStructSize;
        public static int m_iradarStructPos;
        public static int m_bSpotted;
        public static int m_bDormant;
        public static int m_pStudioHdr;
        public static int m_vecOrigin;
        public static int m_vecViewOffset;
        public static int m_vecVelocity;
        public static int m_dwBoneMatrix;
        public static int m_lifeState;
        public static int m_iHealth;
        public static int m_iTeamNum;
        public static int m_bIsQueuedMatchmaking;
        public static int m_flNextAttack;
        public static int m_nTickBase;
        public static int m_bBombPlanted;
        public static int m_hActiveWeapon;
        public static int m_iItemDefinitionIndex;
        public static int m_bIsDefusing;
        public static int m_bHasDefuser;
        public static int m_bIsScoped;
        public static int m_iShotsFired;
        public static int m_aimPunchAngle;
        public static int m_bStartedArming;
        public static int m_bFreezePeriod;
        public static int m_bWarmupPeriod;
        public static int m_totalRoundsPlayed;
        public static int m_fRoundStartTime;
        public static int m_IRoundTime;
        public static int m_iFOV;
        public static int m_iClip1;
        public static int m_szLastPlaceName;
        public static int m_iLastCompID;

        static Offsets()
        {
            StreamReader reader;

            try
            {
                if (File.Exists(@"Offsets\offsets.txt"))
                {
                    LoadOffsetsFromScriptKidDump(@"Offsets\offsets.txt");
                }
                else
                {
                    // Load offsets from my own repo
                    LoadOffsetsFromScriptKidDump(OffsetsURL);
                    // Override with offsets from hazerdumper (if their dump is newer)
                    LoadOffsetsFromHazerDumperDump();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Could not load memory offsets", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Environment.Exit(0);
            }

        }

        private static void LoadOffsetsFromScriptKidDump(string OffsetURL)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(OffsetURL);
            StreamReader reader = new StreamReader(stream);
            var fieldInfos = typeof(Offsets).GetFields();
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                var date = Regex.Match(line, @"\d{4}-\d{2}-\d{2}");
                if (date.Success)
                {
                    LastUpdatedOffsets = date.ToString();
                }

                var match = Regex.Match(line, @"\A(?<name>.+) = (?<value>.+)\z");

                if (!match.Success)
                {
                    continue;
                }

                var fieldValueStr = match.Groups["value"].Value;
                if (!int.TryParse(fieldValueStr, out var fieldValue) &&
                    !int.TryParse(fieldValueStr.Substring(2, fieldValueStr.Length - 2), System.Globalization.NumberStyles.HexNumber, null, out fieldValue))
                {
                    continue;
                }

                // find corresponding field and set value
                var fieldInfo = System.Linq.Enumerable.FirstOrDefault(fieldInfos, fi => string.Equals(fi.Name, match.Groups["name"].Value) && fi.FieldType == typeof(int));
                fieldInfo?.SetValue(default, fieldValue);
            }
        }

        private static void LoadOffsetsFromHazerDumperDump()
        {
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(OffsetsURLHazerDumper);
                StreamReader reader = new StreamReader(stream);
                var fieldInfos = typeof(Offsets).GetFields();
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var date = Regex.Match(line, @"\d{4}-\d{2}-\d{2}");
                    if (date.Success)
                    {
                        DateTime LastUpdatedOffsetsHazer = Convert.ToDateTime(date.ToString());
                        if (LastUpdatedOffsets != null && LastUpdatedOffsets != "")
                        {
                            // If my own offset dump is outdated more than hazer lets use hazerdumper
                            if (Convert.ToDateTime(LastUpdatedOffsets) >= LastUpdatedOffsetsHazer)
                            {
                                return;
                            }
                        }

                    }

                    var match = Regex.Match(line, @"Int32 (?<name>.+) = (?<value>.+);\z");
                    if (!match.Success)
                    {
                        continue;
                    }

                    var fieldValueStr = match.Groups["value"].Value;
                    if (!int.TryParse(fieldValueStr, out var fieldValue) &&
                        !int.TryParse(fieldValueStr.Substring(2, fieldValueStr.Length - 2), System.Globalization.NumberStyles.HexNumber, null, out fieldValue))
                    {
                        continue;
                    }

                    // find corresponding field and set value
                    var fieldInfo = System.Linq.Enumerable.FirstOrDefault(fieldInfos, fi => string.Equals(fi.Name, match.Groups["name"].Value) && fi.FieldType == typeof(int));
                    fieldInfo?.SetValue(default, fieldValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load offsets from hazerdumper");
            }
        }
    }
}
