using Newtonsoft.Json;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptKidAntiCheat
{
    public static class Log
    {
        public static string JsonStorageApiUrl = "https://jsonbox.io/";

        public static string JsonStorageUniqueID = "";

        public static string ReplayListEndpoint = "btW3334E5zHUETVGMdSs";

        public static string LastLogEntry = "";

        public static string LastLogEntryAnalytics = "";

        public static string LastLogEntryDebug = "";

        public static List<PlayerLogEntry> PlayerList = new List<PlayerLogEntry> { };

        public static List<PunishmentLogEntry> Punishments = new List<PunishmentLogEntry> { };

        async public static void CreateJsonLog(string ReplayID, string NickName, string ReplayName)
        {
            /* 
             * Sometimes the replay upload fail so I created a new type of log that is updated & uploaded between every round
             * With this log I can find the players from the match and add them to friends and ask them for their gotv replay if needed.
             */
            try
            {
                if (Program.Debug.DisableJsonLog) return;

                Punishments = new List<PunishmentLogEntry> { };
                PlayerList = new List<PlayerLogEntry> { };

                JsonStorageUniqueID = Helper.MD5Hash(ReplayID + NickName);

                var values = new Dictionary<string, string>
                {
                    { "ReplayID", JsonStorageUniqueID },
                    { "ReplayName", ReplayName },
                    { "Version", Program.version },
                    { "NickName", NickName },
                    { "New", "true" }
                };

                string json = JsonConvert.SerializeObject(values);

                using (var client = new HttpClient())
                {
                    var AddToReplayList = await client.PostAsync(JsonStorageApiUrl + ReplayListEndpoint, new StringContent(json, Encoding.UTF8, "application/json"));
                    var CreateReplayJsonBin = await client.PostAsync(JsonStorageApiUrl + JsonStorageUniqueID, new StringContent(json, Encoding.UTF8, "application/json"));
                    var CreateReplayPlayerListJsonBin = await client.PostAsync(JsonStorageApiUrl + JsonStorageUniqueID + "_manifest", new StringContent(json, Encoding.UTF8, "application/json"));
                    Console.WriteLine("New Json Log Created: " + ReplayName);
                }
            }
            catch (Exception ex)
            {
                AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "CreateJsonLogException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public static void AddEntry(LogEntry Entry)
        {
            string LogMessage = Entry.LogMessage != "" ? Entry.LogMessage : Entry.AnalyticsAction;
            if (Entry.AnalyticsCategory != "") LogMessage = Entry.AnalyticsCategory + " | " + LogMessage;
            if (Entry.AnalyticsLabel != "") LogMessage = LogMessage + " | " + Entry.AnalyticsLabel;

            Console.WriteLine("Log: " + LogMessage);

            if (Program.Debug.DebugLog)
            {
                SaveInDebugLog(LogMessage);
            }

            if (Entry.IncludeTimeAndTick)
            {
                try
                {
                    if(Program.GameProcess.IsValid && Program.GameData.ClientState == 6)
                    {
                        Entry.Time = Program.GameData.MatchInfo.RoundTime;
                        Entry.Round = $"{Program.GameData.MatchInfo.RoundNumber:D2}";
                        Entry.Tick = Helper.GetCurrentTick();
                    }
                }
                catch (Exception ex)
                {
                    AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        IncludeTimeAndTick = false,
                        AnalyticsCategory = "Error",
                        AnalyticsAction = "GetTickAndTimeException",
                        AnalyticsLabel = ex.Message
                    });
                }
            }

            // Save in Analytics
            if (Entry.LogTypes.Contains(LogTypes.Analytics))
            {
                SaveInAnalytics(Entry);
            }

            // Save entry in replay log that goes along with the replay file to google drive
            if (Entry.LogTypes.Contains(LogTypes.Replay))
            {
                SaveInReplayLog(Entry);
            }

            // Save in JSON bin log (backup logging incase replay is never uploaded or corrupt)
            if (Entry.LogTypes.Contains(LogTypes.JsonStorage))
            {
                SaveInJsonStorage(Entry);
            }
        }

        private static void SaveInDebugLog(string LogMessage)
        {
            try
            {
                string DebugLogPath = AppDomain.CurrentDomain.BaseDirectory + "log.txt";
                DateTime DebugLogDateTime = DateTime.Now;

                string DuplicateEntryCheck = LogMessage;

                if (LastLogEntryDebug == DuplicateEntryCheck)
                {
                    return; // Skip duplicates
                }

                LastLogEntryDebug = DuplicateEntryCheck;

                // Write to logfile
                using (StreamWriter sw = File.AppendText(DebugLogPath))
                {
                    sw.WriteLine(DebugLogDateTime.ToString("yyyy-MM-dd HH:mm:ss") + " | " + LogMessage); ;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Manifest IOException");
                Console.WriteLine(e.Message);
            }

        }

        private static void SaveInAnalytics(LogEntry Entry)
        {
            try
            {
                if (Entry.AnalyticsCategory == "") return;

                string Category = Entry.AnalyticsCategory;
                string Action = Entry.AnalyticsAction;
                string Label = Entry.AnalyticsLabel;
                int Value = Entry.AnalyticsValue;

                if (Action == "")
                {
                    Action = Entry.LogMessage;
                }

                string DuplicateEntryCheck = Category + Action;

                if(LastLogEntryAnalytics == DuplicateEntryCheck)
                {
                    return; // Skip duplicates
                }

                LastLogEntryAnalytics = DuplicateEntryCheck;

                Analytics.TrackEvent(Entry.AnalyticsCategory, Action, Label, Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AnalyticsTrackingException");
                Console.WriteLine(ex.Message);
            }
        }

        private static void SaveInReplayLog(LogEntry Entry)
        {
            try
            {
                if (Program.FakeCheat.ReplayMonitor == null)
                {
                    return;
                }

                string RecordingName = Program.FakeCheat.ReplayMonitor.RecordingName;

                if (RecordingName == "")
                {
                    AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        IncludeTimeAndTick = false,
                        AnalyticsCategory = "Error",
                        AnalyticsAction = "RecordingNameNull"
                    });
                    return;
                }

                string logEntry = "";

                if (Entry.IncludeTimeAndTick == true)
                {
                    logEntry = "ROUND: " + Entry.Round +
                                " | TIME: " + Entry.Time +
                                " | TICK: " + Entry.Tick +
                                " | EVENT: " + Entry.LogMessage;
                }
                else
                {
                    logEntry = Entry.LogMessage;
                }

                if (logEntry == LastLogEntry)
                {
                    Console.WriteLine("DuplicateLogEntry");
                    return;
                }

                string LogFile = Helper.getPathToCSGO() + @"\" + RecordingName + ".log";

                // Write to logfile
                using (StreamWriter sw = File.AppendText(LogFile))
                {
                    sw.WriteLine(logEntry);
                    LastLogEntry = logEntry;
                }
            }
            catch (Exception ex)
            {
                AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "SaveInReplayLogException",
                    AnalyticsLabel = ex.Message
                });
            }
        }
        private static void SaveInJsonStorage(LogEntry Entry)
        {
            try
            {
                PunishmentLogEntry JsonEntry = new PunishmentLogEntry();
                JsonEntry.Round = Entry.Round;
                JsonEntry.Time = Entry.Time;
                JsonEntry.Tick = Entry.Tick;
                JsonEntry.Event = Entry.LogMessage;
                Punishments.Add(JsonEntry);
            }
            catch (Exception ex)
            {
                AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "AddJsonLogEntryException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

        public static void AddPlayer(string SteamID)
        {
            // Prepare steamid to be uploade to json storage log
            try
            {
                if (Program.Debug.DisableJsonLog) return;

                if (Program.GameData.MatchInfo.SteamIds != null && Program.GameData.MatchInfo.SteamIds.Contains(SteamID))
                {
                    return;
                }

                PlayerLogEntry Entry = new PlayerLogEntry();

                Entry.SteamID = SteamID;

                PlayerList.Add(Entry);
            }
            catch (Exception ex)
            {
                AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "AddJsonLogPlayerException",
                    AnalyticsLabel = ex.Message
                });
            }

            // Write steamid to manifest file to be sent along with replay file
            try
            {
                if (Program.FakeCheat.ActiveMapClass == null || Program.FakeCheat.ReplayMonitor.RecordingName == "" || Program.FakeCheat.ReplayMonitor.RecordingName == null)
                {
                    return;
                }

                if (Program.GameData.MatchInfo.SteamIds != null && Program.GameData.MatchInfo.SteamIds.Contains(SteamID))
                {
                    return;
                }

                string RecordingName = Program.FakeCheat.ReplayMonitor.RecordingName;
                string PlayerListManifest = Helper.getPathToCSGO() + @"\" + RecordingName + ".manifest.log";

                // Write to logfile
                using (StreamWriter sw = File.AppendText(PlayerListManifest))
                {
                    sw.WriteLine(SteamID);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Manifest IOException");
                Console.WriteLine(e.Message);
            }
        }

        async public static void UploadJsonLog()
        {
            try
            {
                if (Program.Debug.DisableJsonLog) return;

                if (JsonStorageUniqueID == "") return;

                Console.WriteLine(JsonStorageApiUrl + JsonStorageUniqueID);

                if (Punishments != null && Punishments.Count > 0)
                {
                    List<PunishmentLogEntry> Entries = new List<PunishmentLogEntry>(Punishments);
                    Punishments.Clear();
                    string json = JsonConvert.SerializeObject(Entries, Formatting.Indented);

                    using (var client = new HttpClient())
                    {
                        var AddItemsToJsonBin = await client.PostAsync(JsonStorageApiUrl + JsonStorageUniqueID, new StringContent(json, Encoding.UTF8, "application/json"));
                    }
                }

                if (PlayerList != null && PlayerList.Count > 0)
                {
                    List<PlayerLogEntry> Players = new List<PlayerLogEntry>(PlayerList);
                    PlayerList.Clear();
                    string json2 = JsonConvert.SerializeObject(Players, Formatting.Indented);
                    Console.WriteLine(json2);

                    using (var client = new HttpClient())
                    {
                        var AddPlayerListToJsonBin = await client.PostAsync(JsonStorageApiUrl + JsonStorageUniqueID + "_manifest", new StringContent(json2, Encoding.UTF8, "application/json"));
                    }
                }

            }
            catch (Exception ex)
            {
                AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "JsonLogUploadException",
                    AnalyticsLabel = ex.Message
                });
            }
        }
    }
}
