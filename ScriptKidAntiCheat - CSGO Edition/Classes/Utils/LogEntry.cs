using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Utils
{
    public class LogEntry
    {
        public List<LogTypes> LogTypes { get; set; } = new List<LogTypes>();
        public string LogMessage { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IncludeTimeAndTick { get; set; } = false;
        public string Round { get; set; } = "";
        public string Time { get; set; } = "";
        public float Tick { get; set; } = 0;
        public string AnalyticsCategory { get; set; } = "";
        public string AnalyticsAction { get; set; } = "";
        public string AnalyticsLabel { get; set; } = "";
        public int AnalyticsValue { get; set; } = 0;
    }
}
