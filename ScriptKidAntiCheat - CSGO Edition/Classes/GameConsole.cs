using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptKidAntiCheat.Classes
{
    class GameConsole
    {
        public TcpClient client;

        public bool connecting = false;

        public bool connected = false;

        public bool TelnetTestSuccess = false;

        private int telnet_port = 8080;

        private NetworkStream stream;

        private GameConsoleOld BackupMethod = new GameConsoleOld();

        public event EventHandler<ConsoleReadEventArgs> ConsoleRead;

        public GameConsole()
        {
            if (!CheckLaunchOptions())
            {
                if (Program.Debug.ShowDebugMessages)
                {
                    Console.WriteLine("Checking launch options (if you get this more than once = error)");
                }
                EditLaunchOptions();
            }
        }
        public virtual void OnConsoleRead(ConsoleReadEventArgs e)
        {
            EventHandler<ConsoleReadEventArgs> handler = ConsoleRead;
            handler?.Invoke(this, e);
        }

        public bool CheckLaunchOptions()
        {
            bool isReady = true;

            string[] users = Directory.GetDirectories(Helper.getPathToSteam() + @"\userdata");
            foreach (string user in users)
            {
                string localconfig = user + @"\config\localconfig.vdf";

                // Add keybinding to execute our console commands to the player default config
                if (File.Exists(localconfig))
                {

                    // Check if we already tempered with the launch options
                    using (StreamReader sr = new StreamReader(localconfig))
                    {
                        string contents = sr.ReadToEnd();
                        if (!contents.Contains("-netconport " + telnet_port) && contents.Contains(@"""730"""))
                        {
                            isReady = false;
                        }
                    }
                }
            }

            return isReady;
        }

        private void EditLaunchOptions()
        {

            string[] users = Directory.GetDirectories(Helper.getPathToSteam() + @"\userdata");
            foreach (string user in users)
            {
                string localconfig = user + @"\config\localconfig.vdf";

                // Add keybinding to execute our console commands to the player default config
                if (File.Exists(localconfig))
                {

                    bool FoundSoftware = false;
                    bool FoundApps = false;
                    bool FoundCS = false;
                    bool FoundMatch = false;
                    string newLocalConfig = "";

                    var lines = File.ReadLines(localconfig);
                    foreach (var line in lines)
                    {
                        if (line.ToLower().Contains("\"software\""))
                        {
                            FoundSoftware = true;
                        }
                        if (FoundSoftware && line.ToLower().Contains("\"apps\""))
                        {
                            FoundApps = true;
                        }
                        if (FoundApps && line.ToLower().Contains("\"730\""))
                        {
                            FoundCS = true;
                        }

                        if (FoundMatch)
                        {
                            newLocalConfig += line + "\r";
                        }
                        else
                        {
                            if (FoundCS && line.ToLower().Contains("\"launchoptions\""))
                            {
                                newLocalConfig += "\"LaunchOptions\" \"-netconport " + telnet_port + "\"" + "\r";
                                FoundMatch = true;
                            }
                            else if (FoundCS && line.Contains("}"))
                            {
                                newLocalConfig += line.Replace("}", @"""LaunchOptions"" ""-netconport " + telnet_port + @"""
                            }") + "\r";
                                FoundMatch = true;
                            }
                            else
                            {
                                newLocalConfig += line + "\r";
                            }
                        }


                    }

                    if (FoundMatch)
                    {

                        if (Program.Debug.ShowDebugMessages)
                        {
                            Console.WriteLine("Successfully added launch options");
                        }

                        bool CloseProblem = false;

                        // Try kill any running instance of csgo
                        try
                        {
                            Process CSGO = Process.GetProcessesByName("csgo").FirstOrDefault();

                            if (CSGO != null && CSGO.IsRunning())
                            {
                                CSGO.Kill();
                            }
                        }
                        catch (Exception ex)
                        {
                            CloseProblem = true;
                            Log.AddEntry(new LogEntry()
                            {
                                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                IncludeTimeAndTick = false,
                                AnalyticsCategory = "Error",
                                AnalyticsAction = "KillCSGOException",
                                AnalyticsLabel = ex.Message
                            });
                        }

                        // Try kill any running instance of steam
                        try
                        {
                            Process Steam = Process.GetProcessesByName("Steam").FirstOrDefault();

                            if (Steam != null && Steam.IsRunning())
                            {
                                Steam.Kill();
                            }
                        }
                        catch (Exception ex)
                        {
                            CloseProblem = true;
                            Log.AddEntry(new LogEntry()
                            {
                                LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                IncludeTimeAndTick = false,
                                AnalyticsCategory = "Error",
                                AnalyticsAction = "KillSteamException",
                                AnalyticsLabel = ex.Message
                            });
                        }

                        // If force close fails lets tell cheater to restart csgo + steam
                        if(CloseProblem)
                        {
                            MessageBox.Show("Please restart Steam and CSGO for NeuroN to work properly", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }

                        File.WriteAllText(localconfig, newLocalConfig);

                        if (File.Exists(Helper.getPathToSteam() + @"\steam.exe"))
                        {
                            Process.Start(Helper.getPathToSteam() + @"\steam.exe");
                        }

                    }
                }
            }
        }

        public void Connect()
        {
            if (connected || connecting) return;

            connecting = true;

            while (!connected)
            {
                try
                {
                    // Connect to tcp server
                    client = new TcpClient("127.0.0.1", telnet_port);

                    // Get a client stream for reading and writing.
                    stream = client.GetStream();

                    if (client.Connected)
                    {
                        if (Program.Debug.ShowDebugMessages)
                        {
                            Console.WriteLine("TELNET CONNECTED");
                        }

                        connected = true;

                        Task.Run(() =>
                        {
                            ConsoleReader();
                        });

                        SendCommand("echo telnet_success");
                        SendCommand("name");
                    }

                }
                catch (Exception ex)
                {
                    Log.AddEntry(new LogEntry()
                    {
                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                        IncludeTimeAndTick = false,
                        AnalyticsCategory = "Error",
                        AnalyticsAction = "TelnetException",
                        AnalyticsLabel = ex.Message
                    });
                }
            }


        }

        public void ConsoleReader()
        {
            while (true)
            {
                try
                {
                    if (client == null || client.Connected == false)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line != "")
                            {
                                ConsoleReadEventArgs args = new ConsoleReadEventArgs();
                                args.Response = line;
                                OnConsoleRead(args);

                                if (line.Contains("telnet_success"))
                                {
                                    Log.AddEntry(new LogEntry()
                                    {
                                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                        IncludeTimeAndTick = false,
                                        AnalyticsCategory = "Console",
                                        AnalyticsAction = "TelnetSuccess"
                                    });
                                    TelnetTestSuccess = true;
                                }
                                if (line.Contains("Recording to"))
                                {
                                    Log.AddEntry(new LogEntry()
                                    {
                                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                        AnalyticsCategory = "Replays",
                                        AnalyticsAction = "RecordingStarted"
                                    });
                                }
                                if (line.Contains("Completed demo"))
                                {
                                    Log.AddEntry(new LogEntry()
                                    {
                                        LogTypes = new List<LogTypes> { LogTypes.Analytics },
                                        AnalyticsCategory = "Replays",
                                        AnalyticsAction = "CompletedDemo"
                                    });
                                }
                            }
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
                        AnalyticsAction = "ConsoleReadException",
                        AnalyticsLabel = ex.Message
                    });
                }
            }
        }

        public void SendCommand(string Command)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    BackupMethod.SendCommand(Command);
                    return;
                }

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(Command + "\r");

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                // Something went wrong
                Console.WriteLine(Command);
                Log.AddEntry(new LogEntry()
                {
                    LogTypes = new List<LogTypes> { LogTypes.Analytics },
                    IncludeTimeAndTick = false,
                    AnalyticsCategory = "Error",
                    AnalyticsAction = "SendConsoleCmdException",
                    AnalyticsLabel = ex.Message
                });
            }
        }

    }
}
