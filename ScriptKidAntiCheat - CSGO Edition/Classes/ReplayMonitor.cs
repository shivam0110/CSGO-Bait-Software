using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Win32;
using System.Windows.Forms;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptKidAntiCheat
{
    public class ReplayMonitor
    {
        public GoogleDriveUploader GoogleDriveUploader = new GoogleDriveUploader();

        public int PunishmentCounter { get; set; } = 0;

        public int RoundCounter { get; set; } = 0;

        public ReplayMonitor()
        {
            // Generate new google drive token (saved in token.json)
            // GoogleDriveUploader.generateNewToken();

            Program.GameData.MatchInfo.OnMatchNewRound += OnNewMatchRound;
        }

        private void OnNewMatchRound(object sender, EventArgs e)
        {
            RoundCounter++;
            if (RoundCounter > 6 && PunishmentCounter >= 6)
            {
                // Split and upload current replay and reset counters
                RoundCounter = 0;
                PunishmentCounter = 0;
                StopActiveRecording();
                Thread.Sleep(500);
                checkForReplaysToUpload(true);
                Thread.Sleep(1000);
                Program.FakeCheat.ActiveMapClass.StartRecording();
            }
        }

        public void checkForReplaysToUpload(bool skipStopRecording = false)
        {
            if (Program.Debug.DisableGoogleDriveUpload == true) return;

            var CSGO_PATH = Helper.getPathToCSGO();

            try
            {
                if (CSGO_PATH != "")
                {
                    string[] demos = Directory.GetFiles(CSGO_PATH, "*#SHEETER.dem");

                    foreach (string demo in demos)
                    {
                        FileInfo file = new FileInfo(demo);
                        string LogFile = demo.Replace(".dem", ".log");

                        // Analytics.TrackEvent("Replays", "Created");
                        if (Helper.IsFileLocked(file) || file.Length < 1500000)
                        {
                            continue;
                        }

                        // Skip replays without punishments
                        if (File.Exists(LogFile))
                        {
                            try
                            {
                                int punishmentCount = File.ReadLines(LogFile).Select(line => Regex.Matches(line, @"(?i)\bROUND\b").Count).Sum();
                                if (punishmentCount < 1)
                                {
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                // YEET
                            }
                        }

                        if (!skipStopRecording)
                        {
                            StopActiveRecording();
                        }

                        // Let's upload replay to google drive
                        var t = Task.Run(() => {
                            GoogleDriveUploader.UploadFile(file);
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                // YEET
            }

        }

        private void StopActiveRecording()
        {
            // If player is in a match at this point lets stop any ongoing recordings just as a failsafe
            if (Program.GameProcess.IsValid && Program.GameData.ClientState == 6) // 6 = Connected and ingame
            {
                if (!Program.GameProcess.IsValidAndActiveWindow)
                {
                    User32.SetForegroundWindow(Program.GameProcess.Process.MainWindowHandle);
                    Program.GameConsole.SendCommand("stop");
                }
                else if (Program.GameProcess.IsValid)
                {
                    Program.GameConsole.SendCommand("stop");
                }
            }
        }

        private void OnNewReplayCreation(object source, FileSystemEventArgs e)
        {


        }

    }
}
