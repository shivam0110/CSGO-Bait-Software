using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Win32;
using System.Windows.Forms;
using System.Linq;
using System.Text.RegularExpressions;
using ScriptKidAntiCheat.Classes;
using System.Collections.Generic;

namespace ScriptKidAntiCheat
{
    public class ReplayMonitor
    {
        public GoogleDriveUploader GoogleDriveUploader = new GoogleDriveUploader();
        public int PunishmentCounter { get; set; } = 0;
        public string RecordingName { get; set; } = "";
        public long RecordingStarted { get; set; } = 0;

        public bool IsUploading = false;

        public ReplayMonitor()
        {
            // Generate new google drive token (saved in token.json)
            // GoogleDriveUploader.generateNewToken();
        }
        public void StartRecording()
        {
            if (!Program.GameProcess.IsValid || Program.FakeCheat.ActiveMapName == null || Program.FakeCheat.ActiveMapName == "") return;

            Console.WriteLine("AttemptToStartRecording");

            string now = DateTime.UtcNow.ToString("yyyy-MM-dd(HHmmss)");
            long AttemptRecordingStarted = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            string AttemptRecordingName = now + "#(" + Program.FakeCheat.ActiveMapName + ")#sheeter";

            // Start in-eye recording
            Program.GameConsole.SendCommand("record \"" + AttemptRecordingName + "\"");

            Thread.Sleep(1000);

            // Clear the console (sneakyyyy)
            if(Program.GameConsole.TelnetTestSuccess == true)
            {
                Program.GameConsole.SendCommand("clear");
            }

            string DemoFile = Helper.getPathToCSGO() + @"\" + AttemptRecordingName + ".dem";

            // Check if new demo file was created on drive (if not then we can assume record already started)
            if (File.Exists(DemoFile))
            {
                RecordingName = AttemptRecordingName;
                RecordingStarted = AttemptRecordingStarted;

                // Logging
                LogEntry Entry = new LogEntry();
                Entry.LogTypes.Add(LogTypes.Replay);
                Entry.IncludeTimeAndTick = false;
                Entry.LogMessage = "Version: " + Program.version;
                Log.AddEntry(Entry);
                Entry.LogMessage = "Nickname: " + Program.GameData.Player.NickName;
                Log.AddEntry(Entry);
            }
        }

        public void checkForReplaysToUpload()
        {
            Console.WriteLine("Checking for replays to upload");
            if (Program.Debug.DisableGoogleDriveUpload == true || IsUploading) return;

            var CSGO_PATH = Helper.getPathToCSGO();
            int ReplayCount = 0;

            IsUploading = true;

            try
            {
                if (CSGO_PATH != "")
                {
                    string[] demos = Directory.GetFiles(CSGO_PATH, "*#SHEETER.dem");

                    foreach (string demo in demos)
                    {
                        FileInfo file = new FileInfo(demo);
                        string LogFile = demo.Replace(".dem", ".log");
                        int retries = 0;

                        try
                        {
                            while (Helper.IsFileLocked(file))
                            {
                                retries++;
                                if (retries == 3)
                                {
                                    Log.AddEntry(new LogEntry()
                                    {
                                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                        AnalyticsCategory = "Replays",
                                        AnalyticsAction = "FileIsLocked"
                                    });
                                    throw new ArgumentNullException();
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.AddEntry(new LogEntry()
                            {
                                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                IncludeTimeAndTick = false,
                                AnalyticsCategory = "Error",
                                AnalyticsAction = "ReplayFileLockedException",
                                AnalyticsLabel = ex.Message
                            });
                            continue;
                        }

                        // Skip replays without punishments
                        /*
                        try
                        {
                            if (File.Exists(LogFile))
                            {
                                int punishmentCount = File.ReadLines(LogFile).Select(line => Regex.Matches(line, @"(?i)\bROUND\b").Count).Sum();
                                if(punishmentCount < 1)
                                {
                                    Log.AddEntry(new LogEntry()
                                    {
                                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                        AnalyticsCategory = "Replays",
                                        AnalyticsAction = "NoPunishmentsInLog"
                                    });
                                    continue; 
                                }
                            } else
                            {
                                Log.AddEntry(new LogEntry()
                                {
                                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                    AnalyticsCategory = "Replays",
                                    AnalyticsAction = "NoLogFileFound"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("CheckLogForPunishmentsException");
                            Console.WriteLine(ex.Message);
                        }
                        */

                        ReplayCount++;

                        // Let's upload replay to google drive
                        var t = Task.Run(() => {
                            try
                            {
                                GoogleDriveUploader.UploadFile(file);
                            }
                            catch (Exception ex)
                            {
                                Log.AddEntry(new LogEntry()
                                {
                                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                    IncludeTimeAndTick = false,
                                    AnalyticsCategory = "Error",
                                    AnalyticsAction = "GoogleDriveUploaderException",
                                    AnalyticsLabel = ex.Message
                                });
                            }
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "CheckForReplaysException",
                    AnalyticsLabel = ex.Message
                });
            }

            IsUploading = false;

        }

        private void OnNewReplayCreation(object source, FileSystemEventArgs e)
        {
            
           
        }

    }
}
