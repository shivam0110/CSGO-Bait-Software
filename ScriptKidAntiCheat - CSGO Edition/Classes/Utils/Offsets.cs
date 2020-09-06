using System;
using System.IO;
using System.Net;

namespace ScriptKidAntiCheat.Utils
{
    /*
     * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
     */
    public static class Offsets
    {
        public const string OffsetsURL = "https://gitlab.com/sheetergang/csgo-memory-offsets/-/raw/master/offsets.txt";

        public const int MAXSTUDIOBONES = 128; // total bones actually used
        public const float weapon_recoil_scale = 2.0f;

        public static int dwClientState;
        public static int dwClientState_Map;
        public static int dwClientState_ViewAngles;
        public static int dwClientState_State;
        public static int dwLocalPlayer;
        public static int dwEntityList;
        public static int m_bSpotted;
        public static int m_bDormant;
        public static int m_pStudioHdr;
        public static int dwGameRulesProxy;
        public static int dwGlobalVars;
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

        static Offsets()
        {
            var fieldInfos = typeof(Offsets).GetFields();

            StreamReader reader;

            try
            {
                if (File.Exists(@"Offsets\offsets.txt"))
                {
                    reader = new StreamReader(@"Offsets\offsets.txt");
                }
                else
                {
                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead(OffsetsURL);
                    reader = new StreamReader(stream);
                }

                /*
                 * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
                 */
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\A(?<name>.+) = (?<value>.+)\z");
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
                System.Windows.Forms.MessageBox.Show("Could not load memory offsets", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Environment.Exit(0);
            }

        }
    }
}
