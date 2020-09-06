using Microsoft.Win32;
using ScriptKidAntiCheat.Internal;
using ScriptKidAntiCheat.Utils.Maths;
using ScriptKidAntiCheat.Win32;
using SharpDX;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
 * */
namespace ScriptKidAntiCheat.Utils
{

    public static class Helper
    {

        public static int MOUSEEVENTF_MOVE = 0x0001;

        public static int MaxStudioBones = 128; // total bones actually used
        public static double AnglePerPixel { get; set; } = 0.000305334789057573;

        public static bool MouseIsCalibrated = false;

        public static bool HasLocalSounds = false;

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static System.Drawing.Rectangle GetClientRectangle(IntPtr handle)
        {
            return User32.ClientToScreen(handle, out var point) && User32.GetClientRect(handle, out var rect)
                ? new System.Drawing.Rectangle(point.X, point.Y, rect.Right - rect.Left, rect.Bottom - rect.Top)
                : default;
        }

        public static System.Diagnostics.ProcessModule GetProcessModule(this System.Diagnostics.Process process, string moduleName)
        {
            return process?.Modules.OfType<System.Diagnostics.ProcessModule>()
                .FirstOrDefault(a => string.Equals(a.ModuleName.ToLower(), moduleName.ToLower()));
        }

        public static Module GetModule(this System.Diagnostics.Process process, string moduleName)
        {
            var processModule = process.GetProcessModule(moduleName);
            return processModule is null || processModule.BaseAddress == IntPtr.Zero
                ? default
                : new Module(process, processModule);
        }

        public static bool CalibrateMouseSensitivity()
        {
            if (!Program.GameProcess.IsValidAndActiveWindow || !Program.GameData.Player.CanShoot || !Program.GameData.Player.IsAlive())
            {
                return false;
            }

            // Move mouse and calculate how much view angels are changed
            try
            {
                AnglePerPixel = new[] {
                    MeasureAnglePerPixel(100),
                    MeasureAnglePerPixel(-200),
                    MeasureAnglePerPixel(300),
                    MeasureAnglePerPixel(-400),
                    MeasureAnglePerPixel(200),
                }.Average();

                MouseIsCalibrated = true;

                return MouseIsCalibrated;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static double MeasureAnglePerPixel(int pixels)
        {
            // Start view angle
            Thread.Sleep(25);
            Vector3 eyeStart = new Vector3();
            eyeStart = Program.GameData.Player.EyeDirection;
            eyeStart.Z = 0;

            // Move mouse
            SendInput.MouseMove(pixels, 0);

            // End view angle
            Thread.Sleep(25);
            Vector3 eyeEnd = new Vector3();
            eyeEnd = Program.GameData.Player.EyeDirection;
            eyeEnd.Z = 0;

            // Return calibration
            if (eyeStart.X == eyeEnd.X)
            {
                throw new Exception("Calibration failed");
            }

            return eyeEnd.AngleTo(eyeStart) / Math.Abs(pixels);
        }

        // Get view angles to point world
        public static Vector2 GetAimAngles(Vector3 pointWorld)
        {
            var aimDirection = Program.GameData.Player.AimDirection;

            var aimDirectionDesired = (pointWorld - Program.GameData.Player.EyePosition).Normalized();

            return new Vector2
            (
                aimDirectionDesired.AngleToSigned(aimDirection, new Vector3(0, 0, 1)),
                aimDirectionDesired.AngleToSigned(aimDirection, aimDirectionDesired.Cross(new Vector3(0, 0, 1)).Normalized())
            );
        }

        // Translate view angels into how many pixels to move mouse to new target
        public static Point GetAimPixels(Vector2 aimAngles)
        {
            var fovRatio = 90.0 / Program.GameData.Player.Fov;
            return new Point
            (
                (int)Math.Round(aimAngles.X / AnglePerPixel * fovRatio),
                (int)Math.Round(aimAngles.Y / AnglePerPixel * fovRatio)
            );
        }

        public static void SetViewAngleToWorldPoint(Vector3 WorldPoint)
        {
            Vector2 DesiredViewAngle = new Vector2();
            Vector2 aimPixels = new Vector2();
            int MoveMouseX1, MoveMouseY1, MoveMouseX2, MoveMouseY2;

            DesiredViewAngle = Helper.GetAimAngles(WorldPoint);
            aimPixels = Helper.GetAimPixels(DesiredViewAngle);
            MoveMouseX1 = (int)Math.Round(aimPixels.X);
            MoveMouseY1 = (int)Math.Round(aimPixels.Y);
            SendInput.MouseMove(MoveMouseX1, MoveMouseY1);
        }

        public static void AimLock(Vector3 WorldPoint, int LockTime = 1000, Vector3 WorldPointAfter = default)
        {
            Task.Run(() => {
                DateTime startTime, endTime;
                startTime = DateTime.Now;
                int x = 0;
                //Vector2 reset = new Vector2(Program.GameData.Player.ViewAngles.X, Program.GameData.Player.ViewAngles.Y);
                Vector3 reset = Program.GameData.Player.EyePosition;
                while (true)
                {
                    Thread.Sleep(1);

                    x++;
                    // Keep locking aim to target every xx ms
                    if (x >= 10)
                    {
                        SetViewAngleToWorldPoint(WorldPoint);
                        x = 0;
                    }

                    // Cancel after lock time elpased
                    endTime = DateTime.Now;
                    Double elapsedMillisecs = ((TimeSpan)(endTime - startTime)).TotalMilliseconds;
                    if (elapsedMillisecs > LockTime)
                    {
                        // Optional set ViewAngle to something else after lock released (etc used can be used for resetting)
                        if (WorldPointAfter != default)
                        {
                            SetViewAngleToWorldPoint(WorldPointAfter);
                        }
                        break;
                    }
                }
            });
        }

        public static bool IsRunning(this System.Diagnostics.Process process)
        {
            try
            {
                System.Diagnostics.Process.GetProcessById(process.Id);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static string PathToCSGO;

        public static string getPathToCSGO()
        {
            if (PathToCSGO != null)
            {
                return PathToCSGO;
            }

            try
            {
                // Try find in registry
                var RegistryInstallPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam\\").GetValue("SteamPath");

                if (RegistryInstallPath != null)
                {
                    string RegistryInstallPathStr = RegistryInstallPath.ToString().Replace("/", "\\").Replace("/", @"\");
                    if (Directory.Exists(RegistryInstallPathStr + @"\steamapps\common\Counter-Strike Global Offensive\csgo"))
                    {
                        PathToCSGO = RegistryInstallPathStr + @"\steamapps\common\Counter-Strike Global Offensive\csgo";
                        return PathToCSGO;
                    }
                    else if (File.Exists(RegistryInstallPathStr + @"\steamapps\libraryfolders.vdf"))
                    {
                        // Try find seperate steam library
                        StreamReader reader = new StreamReader(RegistryInstallPathStr + @"\steamapps\libraryfolders.vdf");
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var match = System.Text.RegularExpressions.Regex.Matches(line, @"([""'])(?:(?=(\\?))\2.)*?\1");
                            if (match.Count < 2)
                            {
                                continue;
                            }
                            int n;
                            bool isNumeric = int.TryParse(match[0].ToString().Replace(@"""", ""), out n);
                            if (isNumeric && match[1].Length > 0)
                            {
                                var libraryPath = match[1].ToString().Replace(@"""", "");
                                if (Directory.Exists(libraryPath + @"\steamapps\common\Counter-Strike Global Offensive\csgo"))
                                {

                                    PathToCSGO = libraryPath + @"\steamapps\common\Counter-Strike Global Offensive\csgo";
                                    Console.WriteLine(PathToCSGO);
                                    return PathToCSGO;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            // Try find manually
            var csgo_32 = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
            var csgo_64 = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";

            if (Directory.Exists(csgo_64))
            {
                PathToCSGO = csgo_64;
                return PathToCSGO;
            }
            else if (Directory.Exists(csgo_32))
            {
                PathToCSGO = csgo_32;
                return PathToCSGO;
            }

            return "";
        }

        public static string PathToSteam;

        public static string getPathToSteam()
        {
            if (PathToSteam != null)
            {
                return PathToSteam;
            }

            // Try find in registry
            var RegistryInstallPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam\\").GetValue("SteamPath");

            if (RegistryInstallPath != null)
            {
                string RegistryInstallPathStr = RegistryInstallPath.ToString().Replace("/", "\\").Replace("/", @"\");
                if (Directory.Exists(RegistryInstallPathStr))
                {
                    PathToSteam = RegistryInstallPathStr;
                    return PathToSteam;
                }
            }

            var steam_32 = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Steam";
            var steam_64 = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Steam";

            if (Directory.Exists(steam_64))
            {
                PathToSteam = steam_64;
                return PathToSteam;
            }
            else if (Directory.Exists(steam_32))
            {
                PathToSteam = steam_32;
                return PathToSteam;
            }

            return "";
        }

        public static string PlayerNickname;

        public static string getPlayerNickname()
        {
            if (PlayerNickname != null)
            {
                return PlayerNickname;
            }

            var SteamPath = getPathToSteam();
            if (SteamPath != "")
            {
                if (Directory.Exists(SteamPath + @"\userdata"))
                {
                    string newestConfig = "";
                    DateTime lastModified = default;
                    string[] users = Directory.GetDirectories(SteamPath + @"\userdata");
                    foreach (string user in users)
                    {
                        string configPath = user + @"\730\local\cfg\config.cfg";
                        if (File.Exists(configPath))
                        {
                            DateTime configLastModified = File.GetLastWriteTime(configPath);
                            if (lastModified == default || lastModified < configLastModified)
                            {
                                lastModified = configLastModified;
                                newestConfig = configPath;
                            }
                        }
                    }

                    if (newestConfig != "")
                    {
                        try
                        {
                            string readText = File.ReadAllText(newestConfig);

                            var match = Regex.Match(readText, "(?<=^name\\s\").*(?=\")", RegexOptions.Multiline);

                            if (!match.Success || match.Value.Length < 4)
                            {
                                return "Sheeter";
                            }

                            // Hide full nickname
                            PlayerNickname = match.Value.Substring(0, match.Value.Length - 3) + "---";

                            return PlayerNickname;
                        }
                        catch (IOException e)
                        {

                        }
                    }
                }
            }

            return "Sheeter";
        }

        public static int pointerI = 0;
        public static string pointers = "";

        public static void TripWireMaker(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'v')
            {
                Vector3 WorldPoint = new Vector3(-1245, -621, -65);
                Helper.SetViewAngleToWorldPoint(WorldPoint);
            }


            // Reset current pointer when pressing C
            if (e.KeyChar == 'c')
            {
                pointers = "";
                pointerI = 0;
            }

            // Generate pointer when pressing p (after 4 pointers generated it will be printed in console)
            if (e.KeyChar == 'p')
            {

                // Find dwLocalPlayer base address in csgo memory
                var baseAddressdwLocalPlayer = Program.GameProcess.ModuleClient.Read<IntPtr>(Offsets.dwLocalPlayer);
                if (baseAddressdwLocalPlayer != IntPtr.Zero)
                {
                    // Read player location on map
                    var origin = Program.GameProcess.Process.Read<Vector3>(baseAddressdwLocalPlayer + Offsets.m_vecOrigin);
                    var viewOffset = Program.GameProcess.Process.Read<Vector3>(baseAddressdwLocalPlayer + Offsets.m_vecViewOffset);
                    var eyePosition = origin + viewOffset;

                    // Add pointer to string in correct format to be copied for tripwire setup
                    pointerI = pointerI + 1;
                    pointers += "x" + pointerI.ToString() + " = " + (int)eyePosition.X + ", y" + pointerI.ToString() + " = " + (int)eyePosition.Y + ",";

                    // Print the pointers generated by pressing p 4 times
                    if (pointerI == 4)
                    {
                        Console.WriteLine("######################################");
                        Console.WriteLine("TRIPWIRE LOCATION:");
                        Console.WriteLine(pointers);
                        Console.WriteLine("z = " + (int)eyePosition.Z);
                        Console.WriteLine("######################################");
                        pointers = "";
                        pointerI = 0;
                    }
                    else
                    {
                        // New line
                        pointers += "\r";
                    }

                }
            }

        }

    }
}
