using ScriptKidAntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Classes.Utils
{
    public static class PlayerConfig
    {
        static string configPath { get; set; } = "";

        static bool IsResetting { get; set; } = false;

        public static void ResetConfig()
        {
            if(IsResetting == false)
            {
                IsResetting = true;

                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (Program.GameProcess.IsValid)
                            {
                                Program.GameConsole.SendCommand("cl_chatfilters 63");
                                Program.GameConsole.SendCommand("exec configbackup.cfg; host_writeconfig resetcheck.cfg");

                                Thread.Sleep(2500); // Wait for resetcheck.cfg to be written

                                // If resetcheck.cfg exists on drive then we know exec was also executed
                                if (configPath != "" && File.Exists(configPath + "resetcheck.cfg"))
                                {
                                    // Delete and return reset should have been successful at this point
                                    IsResetting = false;
                                    File.Delete(configPath + "resetcheck.cfg");
                                    return;
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
                                AnalyticsAction = "ResetConfigException",
                                AnalyticsLabel = ex.Message
                            });
                        }

                        // Try again in 5 seconds if something failed (maybe they we're tab'ed out etc)
                        Thread.Sleep(5000);
                    }
                });
            }

        }

        public static void CreateBackup()
        {
            Task.Run(() =>
            {
                try
                {
                    if (Program.GameProcess.IsValidAndActiveWindow)
                    {
                        // Create copy of player cfg (used for resetting punishments)
                        Program.GameConsole.SendCommand("host_writeconfig configbackup.cfg");

                        Thread.Sleep(1000);

                        var SteamPath = Helper.getPathToSteam();

                        // Check if userdata dir exists
                        if (SteamPath != "" && Directory.Exists(SteamPath + @"\userdata"))
                        {
                            string[] users = Directory.GetDirectories(SteamPath + @"\userdata");

                            // Loop through all steam account folders
                            foreach (string user in users)
                            {
                                // Check for our newly created configbackup to determine what account is used
                                if (File.Exists(user + @"\730\local\cfg\configbackup.cfg"))
                                {
                                    configPath = user + @"\730\local\cfg\";
                                    return; // Found the active config folder
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
                        AnalyticsAction = "CreateBackupException",
                        AnalyticsLabel = ex.Message
                    });
                }

                // If we reached this far somethin went wrong, try again in 5 seconds
                Thread.Sleep(5000);
                CreateBackup();
            });

        }
    }
}
