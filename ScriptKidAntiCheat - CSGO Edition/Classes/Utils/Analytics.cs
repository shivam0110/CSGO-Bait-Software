using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKidAntiCheat.Utils
{
    static class Analytics
    {
        private static readonly HttpClient client = new HttpClient();

        private static string UAID = "";

        private static string cid; 

        static Analytics()
        {
            cid = "CID_HERE";
        }

        async public static void TrackEvent(string Category, string Action, string label = "", int value = 0)
        {
            if (Program.Debug.SkipAnalyticsTracking || UAID == "" || cid == "") return;

            var values = new Dictionary<string, string>
            {
                { "v", "1" },
                { "tid", UAID },
                { "cid", cid.ToString() },
                { "t", "event" },
                { "ec", Category },
                { "ea", Action }
            };

            if(label != "")
            {
                values.Add("el", label);
            }

            if(value != 0)
            {
                values.Add("ev", value.ToString());
            }

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("http://www.google-analytics.com/collect", content);

            var responseString = await response.Content.ReadAsStringAsync();
        }
    }
}
