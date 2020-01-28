using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlagueCast
{
    static class Program
    {
        public const string url = "https://3g.dxy.cn/newh5/view/pneumonia";
        public const string urlnews = "http://lab.isaaclin.cn/nCoV/api/news?num=40";
        public const string urloverall = "http://lab.isaaclin.cn/nCoV/api/overall";

        public static Form1 form1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            Application.Run(new Form1());

            //String json = Utils.SearchJson(Utils.httpGet(url), "window.getTimelineService");
            //List<NewsItem> news = JsonConvert.DeserializeObject<List<NewsItem>>(json);

            //news.ForEach(n => Console.WriteLine(n.pubDateStr + "\t" + n.title));
            //Console.ReadKey();
        }
    }
}
