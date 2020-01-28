using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlagueCast
{
    static class Utils
    {
        private static WebClient wc = new WebClient() { Encoding = Encoding.UTF8 };

        public static string httpGet(string url) {
        return wc.DownloadString(url);
            //try
            //{
            //    string curlpath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "libs", "curl.exe");
            //    ProcessStartInfo psi = new ProcessStartInfo(curlpath, "-t 10 -H 'X-Accel-Buffering: no' " + url);
            //    psi.RedirectStandardInput = true;
            //    psi.RedirectStandardOutput = true;
            //    psi.RedirectStandardError = true;
            //    psi.StandardOutputEncoding = Encoding.UTF8;
            //    psi.StandardErrorEncoding = Encoding.UTF8;
            //    psi.UseShellExecute = false;
            //    psi.CreateNoWindow = true;
            //    Process ps = Process.Start(psi);
            //    Task<string> result = ps.StandardOutput.ReadToEndAsync();
            //    Task<string> error = ps.StandardError.ReadToEndAsync();
            //    ps.WaitForExit();
            //    if (ps.ExitCode == 0)
            //    {
            //        return result.Result;
            //    }
            //    else {
            //        throw new Exception("Process exited with code "+ps.ExitCode+", message\r\n"+error.Result);
            //    }
            //}
            //catch (Exception ex) {
            //    Console.Write(ex.ToString());
            //    return null;
            //}
        }

        public static string SearchJson(string html, string begin) {
            int beginIndex = html.IndexOf(begin);
            if (beginIndex == -1) { return null; }
            beginIndex += begin.Length;

            char[] begins = "[{".ToCharArray();
            int small = 0, medium = 0;
            int ptr = beginIndex;
            while (!begins.Contains(html[ptr])) {
                ptr++;
            }
            StringBuilder sb = new StringBuilder();
            bool insideComma = false;
            bool ignoreBackSlash = false;
            do
            {
                if (!ignoreBackSlash)
                {
                    if (html[ptr] == '\"') { insideComma = !insideComma; }
                    if (!insideComma)
                    {
                        if (html[ptr] == '[') { small++; }
                        if (html[ptr] == '{') { medium++; }
                        if (html[ptr] == ']') { small--; }
                        if (html[ptr] == '}') { medium--; }
                    }
                    else
                    {
                        if (html[ptr] == '\\') { ignoreBackSlash = false; }
                    }
                }
                else
                {
                    ignoreBackSlash = false;
                }
                sb.Append(html[ptr]);
                ptr++;
            }
            while (small != 0 || medium != 0);
            return sb.ToString();
        }

        public static DateTime parstUnixTime(long time) {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(time/1000);
            return dt;
        }

    }

}
