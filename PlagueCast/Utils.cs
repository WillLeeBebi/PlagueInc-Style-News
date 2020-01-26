using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PlagueCast
{
    static class Utils
    {
        public static string httpGet(string url) {
            try
            {
                HttpWebRequest req = BomberUtils.MakeHttpGet(url, "");
                return BomberUtils.GetHttpResponse(req);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                return null;
            }
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


    public static class BomberUtils
    {
        

        public static string useragent = "Mozilla/5.0 (Linux; Android 9; PH-1 Build/PPR1.180610.091; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/66.0.3359.126 MQQBrowser/6.2 TBS/044807 Mobile Safari/537.36 V1_AND_SQ_8.0.8_1218_YYB_D QQ/8.0.8.4115 NetType/WIFI WebP/0.3.0 Pixel/1312 StatusBarHeight/151";

        public static string DictionaryToHttpKeyValue(Dictionary<string, string> dic)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            return builder.ToString();
        }
        
        public static HttpWebRequest MakeHttpGet(string url, string httpKeyValue)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "?" + httpKeyValue);
            req.Timeout = 9999;
            req.Method = "GET";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            req.UserAgent = useragent;
            req.Headers.Add("Accept-Language", "zh,zh-CN;q=0.9;q=0.9");
            req.Headers.Add("Accept-Encoding", "identity");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Headers.Add("Cache-Control", "no-cache");
            req.Headers.Add("Pragma", "no-cache");
            req.Headers.Add("DNT", "1");
            req.Headers.Add("TE", "Trailers");

            //req.Connection = "Keep-Alive";
            req.AllowAutoRedirect = false;
            return req;
        }
        
        public static string GetHttpResponse(HttpWebRequest req)
        {
            string result = "";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                stream.ReadTimeout = 9999;
               
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                
                return result;
            }
            catch (WebException ex)
            {
                if (null == ex.Response)
                {
                    throw ex;
                }
                if ((int)((HttpWebResponse)ex.Response).StatusCode < 400)
                {
                    return ex.Message;
                }
                throw ex;
            }
        }



    }

}
