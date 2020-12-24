using Gma.System.MouseKeyHook;
using ScriptKidAntiCheat.Classes.Utils;
using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Security.Cryptography;
using System.Windows.Forms;
using ScriptKidAntiCheat.Data;

namespace ScriptKidAntiCheat.Classes
{
    public class FakeCheat
    {
        public ReplayMonitor ReplayMonitor;
        public Map ActiveMapClass { get; set; }

        public bool PunishmentsLoaded = false;

        public string ActiveMapName = "";

        public bool ConfigBackupCreated = false;

        public bool JsonLogCreated = false;

        public bool HasConnectedToGame = false;

        public bool ConsoleGatewayInit;

        public bool DisableCSGOESCMenu = false;

        public bool errorsFound = false;

        public string CurrentMatchID = null;

        public DateTime startTime;

        public DateTime endTime;

        private GameProcess GameProcess;

        private GameConsole GameConsole;

        private GameData GameData;

        private Debug Debug;

        public int ClientState { get; set; } = -1; // Don't change this

        public FakeCheat()
        {
            GameProcess = Program.GameProcess;
            GameConsole = Program.GameConsole;
            GameData = Program.GameData;
            Debug = Program.Debug;

            Log.AddEntry(new LogEntry() {
                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                IncludeTimeAndTick = false,
                AnalyticsCategory = "FakeCheat",
                AnalyticsAction = "Loaded"
            });

            MouseHook.Start();

            // Start replay monitor
            ReplayMonitor = new ReplayMonitor();

            // Event that fires on each new round
            GameData.MatchInfo.OnMatchNewRound += OnNewMatchRound;

            Program.m_GlobalHook.KeyDown += DenyESC;

            Program.m_GlobalHook.OnCombination(new Dictionary<Combination, Action>
            {
                {Combination.FromString("F5"), () => {
                    PrintDebugData();
                 }},
                {Combination.FromString("F6"), () => {
                    Program.GameConsole.SendCommand("say Debug info: TripWires Reset");
                    Program.FakeCheat.ActiveMapClass.resetTripWires();
                 }},
                {Combination.FromString("F7"), () => {
                    MessageBox.Show("FakeCheat Terminated", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    System.Windows.Forms.Application.Exit();
                 }},
            });

            // Setup hotkeys to log player location on map to be used for TripWires
            if (Program.Debug.AllowLocal)
            {
                Program.m_GlobalHook.KeyPress += Helper.TripWireMaker;
            }

            // Main ticker
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        tick();
                        Thread.Sleep(500);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("MainThreadException");
                }
            });
        }
        private void PrintDebugData()
        {
            if (Program.Debug.DebugLog != true) return;
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "-----------------------------------"
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "IsValid: " + (Program.GameProcess.IsValid ? "true" : "false")
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "HasConnectedToGame: " + (HasConnectedToGame ? "true" : "false")
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "IsMatchmaking: " + (Program.GameData.MatchInfo.IsMatchmaking ? "true" : "false")
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "ClientState: " + Program.GameData.ClientState
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "NickName: " + Program.GameData.Player.NickName
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "MatchID: " + Program.GameData.MatchInfo.MatchID
            });
            Log.AddEntry(new LogEntry()
            {
                LogTypes = new List<LogTypes> { LogTypes.Debug },
                LogMessage = "-----------------------------------"
            });
        }

        private void DenyESC(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape && Program.GameProcess.IsValidAndActiveWindow && Program.GameConsole.connected && Program.GameData.ClientState == 6 && DisableCSGOESCMenu)
            {
                Program.GameConsole.SendCommand("escape");
            }
        }

        private void tick()
        {
            try
            {
                bool TimeToReset = false;

                // Detect if MatchId has changed (new matchmaking game)
                if(MatchIdHasChanged() == true)
                {
                    JsonLogCreated = false;
                    TimeToReset = true;
                }

                // Detect if ClientState has changed
                if (ClientStateHasChanged() == true)
                {
                    // Fully Connected to a new game
                    if(ClientState == 6)
                    {
                        TimeToReset = true;
                    }

                    // In menues or csgo not running
                    if (ClientState == 0 || ClientState == -1)
                    {
                        // Try update json storage log
                        Log.UploadJsonLog();
                        // Try upload any existing replays
                        ReplayMonitor.checkForReplaysToUpload();
                    }
                }

                // Reset everything
                if (TimeToReset) Reset();

                // Setup console gateway
                if (ConsoleGatewayInit == false)
                {
                    CreateConsoleGateway();
                }

                // Create cheater config backup (used for resetting some punishments)
                if (ConfigBackupCreated == false)
                {
                    CreatePlayerConfigBackup();
                }

                // Create json log (backup logging sent between each new round)
                if (JsonLogCreated == false)
                {
                    CreateNewJsonLog();
                }

                // Load current map class and its punishments
                if (PunishmentsLoaded == false)
                {
                    LoadPunishments();
                }

                // Calibrate mouse (used for some punishments to move the mouse to correct position in 3d space)
                if (Helper.MouseIsCalibrated == false)
                {
                    AttemptMouseCalibration();
                }

                checkForErrors();
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "MainTickException",
                    AnalyticsLabel = ex.Message
                });
                ReplayMonitor.checkForReplaysToUpload();
            }

        }

        private void OnNewMatchRound(object sender, EventArgs e)
        {
            if (GameData.MatchInfo.IsMatchmaking == false && Debug.AllowLocal != true) return;

            Console.WriteLine("New round detected");

            // Try start new pov replay recording
            try
            {
                ReplayMonitor.StartRecording();
            }
            catch (Exception ex)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "NewRoundStartRecordingException",
                    AnalyticsLabel = ex.Message
                });
            }

            // Try update json storage log
            Log.UploadJsonLog();

        }

        private void Reset()
        {
            Console.WriteLine("FullReset");
            if (ActiveMapClass != null)
            {
                ActiveMapClass.Dispose();
                ActiveMapClass = null;
            }
            PunishmentsLoaded = false;
            HasConnectedToGame = false;
            errorsFound = false;
        }

        private void CreateConsoleGateway()
        {
            if (GameProcess.IsValidAndActiveWindow)
            {
                // Telnet connection with game console
                Task.Run(() =>
                {
                    GameConsole.Connect();
                });
                ConsoleGatewayInit = true;
            }
        }

        private bool ClientStateHasChanged()
        {
            int CurrentClientState = GameProcess.IsValid ? GameData.ClientState : -1;

            if (CurrentClientState != ClientState)
            {
                ClientState = CurrentClientState;
                return true;
            }

            return false;
        }

        private bool MatchIdHasChanged()
        {
            if (GameProcess.IsValid)
            {
                string MatchID = GameData.MatchInfo.MatchID;

                if(CurrentMatchID != MatchID)
                {
                    CurrentMatchID = MatchID;
                    return true;
                }
                
            }

            return false;
        }

        private void CreatePlayerConfigBackup()
        {
            if (GameProcess.IsValidAndActiveWindow)
            {
                PlayerConfig.CreateBackup();
                ConfigBackupCreated = true;
            }
        }
        private void CreateNewJsonLog()
        {
            if (GameProcess.IsValid == false) return;
            if (HasConnectedToGame == false) return;
            if (ActiveMapName == null || ActiveMapName == "") return;
            if (GameData.MatchInfo.IsMatchmaking == false && Debug.AllowLocal == false) return;
            
            string NickName = GameData.Player.NickName;
            string MatchID = GameData.MatchInfo.MatchID;

            if (NickName != null && NickName != "")
            {
                if (MatchID != null)
                {
                    string ReplayName = NickName + " | " + ActiveMapName;
                    string ReplayID = MatchID.ToString();

                    // Create JSON Log (logged online)
                    Log.CreateJsonLog(ReplayID, NickName, ReplayName);

                    CurrentMatchID = MatchID;
                    JsonLogCreated = true;
                }
            }
        }

        private void LoadPunishments()
        {
            if (GameProcess.IsValidAndActiveWindow == false) return;

            string NickName = GameData.Player.NickName;
            string MatchID = GameData.MatchInfo.MatchID;

            if (GameData.Player.IsAlive() == false) return;
            if (GameData.ClientState != 6) return;
            if (NickName == "") return; 

            if(Debug.AllowLocal != true)
            {
                if (GameData.MatchInfo.IsMatchmaking == false) return;
                if (MatchID == null) return;
            }

            if(setupPunishmentsAndTripWires())
            {
                HasConnectedToGame = true;
                PunishmentsLoaded = true;
            }
            
        }

        private void AttemptMouseCalibration()
        {
            bool calibrateMouse = false;

            if (startTime == null)
            {
                calibrateMouse = true;
            }
            else
            {
                endTime = DateTime.Now;
                Double elapsedMillisecs = ((TimeSpan)(endTime - startTime)).TotalMilliseconds;
                // Retry every 5 seconds
                if (elapsedMillisecs > 5000)
                {
                    calibrateMouse = true;
                }
            }

            if (calibrateMouse)
            {
                startTime = DateTime.Now;
                Helper.CalibrateMouseSensitivity();
            }
        }

        private void checkForErrors()
        {
            if (GameProcess.IsValidAndActiveWindow)
            {
                if (Debug.SkipAnalyticsTracking || errorsFound) return;

                string MatchID = GameData.MatchInfo.MatchID;

                // Check MatchID
                if (GameData.ClientState == 6 && GameData.MatchInfo.IsMatchmaking && (MatchID == null && Debug.AllowLocal == false))
                {
                    GameConsole.SendCommand("status");

                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        AnalyticsCategory = "Error",
                        AnalyticsAction = "MissingMatchID"
                    });

                    errorsFound = true;
                }

                // Check Telnet
                if (GameData.ClientState == 6 && GameConsole.TelnetTestSuccess == false)
                {
                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        AnalyticsCategory = "Error",
                        AnalyticsAction = "TelnetError"
                    });
                    errorsFound = true;
                }

                if(GameData.MatchInfo.IsMatchmaking && GameData.ClientState == 6)
                {
                    // Check ActiveMap
                    if (ActiveMapName == "")
                    {
                        Log.AddEntry(new LogEntry()
                        {
                            LogTypes = new List<LogTypes> { LogTypes.Analytics },
                            AnalyticsCategory = "Error",
                            AnalyticsAction = "MissingMapName"
                        });
                        errorsFound = true;
                    }

                    // Check PlayerName
                    if (GameData.Player.NickName == "")
                    {
                        Log.AddEntry(new LogEntry()
                        {
                            LogTypes = new List<LogTypes> { LogTypes.Analytics },
                            IncludeTimeAndTick = false,
                            AnalyticsCategory = "Error",
                            AnalyticsAction = "MissingNickName"
                        });
                        errorsFound = true;
                    }
                }
            }
        }

        private bool setupPunishmentsAndTripWires()
        {

            string MemMapName = Program.GameData.MatchInfo.MapName;

            if (MemMapName == null || MemMapName == "" || MemMapName.ToLower().Contains("de_") == false)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "InvalidMapName",
                    AnalyticsLabel = MemMapName
                });
                return false; // Fail
            }

            ClientState = 6;
            HasConnectedToGame = true;
            ActiveMapName = MemMapName;
            string MatchID = Program.GameData.MatchInfo.MatchID;

            if (MatchID != null)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "MatchID",
                    AnalyticsAction = MatchID
                });
            }

            // Start recording
            Program.FakeCheat.ReplayMonitor.StartRecording();

            Type MapClass = Type.GetType("ScriptKidAntiCheat." + ActiveMapName);

            if(MapClass != null)
            {
               ActiveMapClass = (Map)Activator.CreateInstance(MapClass);
            } else
            {
               ActiveMapClass = new GenericMap();
            }

            Console.WriteLine(ActiveMapName);

            if (ActiveMapClass != null)
            {
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    AnalyticsCategory = "MapLoads",
                    AnalyticsAction = ActiveMapName
                });
            }

            return true; // Success
        }

    }
}
