using ScriptKidAntiCheat.Utils;
using ScriptKidAntiCheat.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ScriptKidAntiCheat
{
    /*
     * This is the old method of sending console commands (writing console commands to config file + simulate key press on key binding that execute that cfg)
     * I keep this method as fallback if for some reason the telnet fails on cheaters computer
     * */
    public class GameConsoleOld
    {
        private System.Timers.Timer writeTimer;

        private System.Timers.Timer readTimer;

        private string CFG_PATH;

        private FileInfo CFG_INFO;

        private bool isWriting = false;

        private List<string> CommandQueue = new List<string>();

        public static bool cfgIsReady = true;

        public GameConsoleOld()
        {
            // Lets add required keybinding to users cs go config on first setup
            if (!checkIfCfgIsReady())
            {
                setupUserConfigs();
            }

            // Create cfg file used to send commands to console using exec
            CFG_PATH = Helper.getPathToCSGO() + @"\cfg\cheater.cfg";

            // Check for locally installed fake sounds (used for watching the replays with fake sounds)
            if (File.Exists(Helper.getPathToCSGO() + @"\sound\flashed.wav"))
            {
                Helper.HasLocalSounds = true;
            }

            if (!File.Exists(CFG_PATH))
            {
                File.Create(CFG_PATH);
            }

            CFG_INFO = new FileInfo(CFG_PATH);

            // Start timer that writes our commands to our cheater.cfg
            writeTimer = new System.Timers.Timer(100);
            writeTimer.Elapsed += fileWriter;
            writeTimer.AutoReset = true;
            writeTimer.Enabled = true;

            // Start timer that will read console output
            readTimer = new System.Timers.Timer(30000);
            readTimer.Elapsed += fileReader;
            readTimer.AutoReset = true;
            readTimer.Enabled = true;
        }

        private void setupUserConfigs()
        {

            string[] users = Directory.GetDirectories(Helper.getPathToSteam() + @"\userdata");
            foreach (string user in users)
            {
                string userConfig = user + @"\730\local\cfg\config.cfg";

                // Add keybinding to execute our console commands to the player default config
                if (File.Exists(userConfig))
                {

                    // Add our keybinding to player cfg
                    using (StreamWriter sw = File.AppendText(userConfig))
                    {
                        // Bind F9 in player config to exec our cheater.cfg file
                        sw.WriteLine("\rbind \"F9\" \"exec cheater.cfg\"");
                    }
                }
            }
        }

        public bool checkIfCfgIsReady()
        {
            bool isReady = true;

            string[] users = Directory.GetDirectories(Helper.getPathToSteam() + @"\userdata");
            foreach (string user in users)
            {
                string userConfig = user + @"\730\local\cfg\config.cfg";

                // Add keybinding to execute our console commands to the player default config
                if (File.Exists(userConfig))
                {

                    // Check if we already tempered with the config
                    using (StreamReader sr = new StreamReader(userConfig))
                    {
                        string contents = sr.ReadToEnd();
                        if (!contents.Contains("cheater.cfg"))
                        {
                            isReady = false;
                        }
                    }
                }
            }

            cfgIsReady = isReady;

            return isReady;
        }

        public void SendCommand(string Command)
        {
            CommandQueue.Add(Command);
        }

        private void fileWriter(Object source, ElapsedEventArgs e)
        {
            // Check if we have any commands queued and file is not being written to already
            if (Helper.IsFileLocked(CFG_INFO) || isWriting || CommandQueue.Count < 1)
            {
                return;
            }

            // Write our commands to cheater.cfg
            writeToCFG();
        }

        /* This is probably the ugliest thing i ever coded... but since changing to the telnet
         * method i rely on reading the console for some things to work so had to add a super hacky way to
         * read the console if for some reason the cheater telnet method is not working and it has to use this backup way */
        private void fileReader(Object source, ElapsedEventArgs e)
        {
            try
            {
                if (Program.GameConsole.client != null && Program.GameConsole.client.Connected == true) return;

                if (Helper.PathToCSGO == "") return;

                ConsoleReadEventArgs args = new ConsoleReadEventArgs();

                SendCommand("condump; clear;");

                Thread.Sleep(1000);

                // dump console to file
                string dumpPath = Helper.PathToCSGO + @"\condump000.txt";

                if (File.Exists(dumpPath))
                {
                    // Read condump line by line
                    StreamReader reader = new StreamReader(dumpPath);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != "")
                        {
                            // Simulate by sending line back to new method and pretend its read like it was via telnet
                            args.Response = line;
                            Program.GameConsole.OnConsoleRead(args);
                        }
                    }

                    reader.Close();

                    Thread.Sleep(1000);

                    // delete con dump
                    File.Delete(Helper.PathToCSGO + @"\condump000.txt");
                }

            }
            catch (Exception ex)
            {
 
            }
            
        }

        private void writeToCFG()
        {
            if (!Program.GameProcess.IsValidAndActiveWindow) return;

            isWriting = true;

            string combinedCommands = "";
            foreach (string Command in CommandQueue.ToList())
            {
                if (!Command.EndsWith(";"))
                {
                    combinedCommands += Command + ";";
                }
                else
                {
                    combinedCommands += Command;
                }
                CommandQueue.Remove(Command);
            }

            try
            {
                // Write commands to our cheater.cfg file
                using (var sw = new StreamWriter(CFG_PATH, false))
                {
                    sw.WriteLine(combinedCommands);
                    sw.Close();
                }

                // Focus csgo
                if (Program.GameProcess.IsValid && !Program.GameProcess.IsValidAndActiveWindow)
                {
                    User32.SetForegroundWindow(Program.GameProcess.Process.MainWindowHandle);
                }

                // Trigger console exec by simulating keypress F9
                SendInput.KeyPress(KeyCode.F9);
            }
            catch (IOException e)
            {

            }

            isWriting = false;

        }
    }
}
