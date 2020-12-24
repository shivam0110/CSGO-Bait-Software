using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Utils
{
    public class Debug
    {

        public bool AllowLocal { get; set; } = false;

        public bool AllowInWarmup { get; set; } = false;

        public bool IgnoreActivateOnRound { get; set; } = false;

        public int TripWireStage { get; set; } = 0;

        public bool DisableRunInBackground { get; set; } = false;

        public bool DisableGoogleDriveUpload { get; set; } = false;

        public bool DisableJsonLog { get; set; } = false;

        public bool DisableAcceptConditions { get; set; } = false;

        public bool ShowDebugMessages { get; set; } = false;

        public bool SkipAnalyticsTracking { get; set; } = false;

        public bool SkipInitDelay { get; set; } = false;

        public bool DebugLog { get; set; } = false;

        public Debug()
        {
            if (File.Exists("debug.txt"))
            {
                var fieldInfos = typeof(Debug).GetFields();

                StreamReader reader = new StreamReader(@"debug.txt");

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = Regex.Match(line, @"\A(?<name>.+)=(?<value>.+)\z");
                    if (!match.Success)
                    {
                        continue;
                    }

                    var key = Regex.Replace(match.Groups["name"].Value, @"\s+", "");
                    var value = Regex.Replace(match.Groups["value"].Value, @"\s+", "");
                    int n;
                    bool isNumeric = int.TryParse(value, out n);

                    if (value.ToLower() == "true" || value.ToLower() == "false")
                    {
                        GetType().GetProperty(key).SetValue(this, value == "true");
                    } else if(isNumeric)
                    {
                        GetType().GetProperty(key).SetValue(this, n);
                    }
                }
            } 

        }
    }
}
