using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyGDIFramework;
using Newtonsoft.Json;
using System.Net;

namespace PlagueCast
{
    public partial class Form1 : Form
    {
        public static List<NewsItem> newsItems = new List<NewsItem>();

        FrmNewsList frmNewsList;

        public List<string> marqueeContents = new List<string>();
        int marqueePtr = -1;
        string current = "正在获取新闻...";

        GdiSystem gdi;
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            gdi = new GdiSystem(this);
            init();
            draw();
            gdi.UpdateWindow();
        }
        private void renderTimer_Tick(object sender, EventArgs e)
        {
            draw();
            gdi.UpdateWindow();
        }



        Image bg = Properties.Resources.bg_preview;
        Image btn = Properties.Resources.btn_expand;

        Image marqueeSurface;
        Graphics marqueeGraphics;


        void init() {
            marqueeSurface = new Bitmap(conMarquee.Width, conMarquee.Height);
            marqueeGraphics = Graphics.FromImage(marqueeSurface);
            marqueeGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            marFont = conMarquee.Font;
            marqueePos = conMarquee.Width;
            read(marqueeGraphics);
            frmNewsList = new FrmNewsList();
            frmNewsList.Show();
            frmNewsList.Left = this.Left + ptListBegin.Left;
            frmNewsList.Top = this.Top + ptListBegin.Top;
            frmNewsList.Visible = false;

            bg = new Bitmap(bg, Width, Height);
        }

        void draw() {
            Graphics g = gdi.Graphics;
            float w = Width;
            float h = Height;
            g.Clear(Color.Transparent);
            g.DrawImage(bg, 0, 0, w,h);
            drawMarquee();
            g.DrawImage(marqueeSurface, conMarquee.Location);
            DrawUtils.drawRotateImg(g, btn, rotate,btnExpand.Left+btnExpand.Width/2,btnExpand.Top+btnExpand.Height/2, btnExpand.Width, btnExpand.Height);

        }

        int marqueePos=0;
        Font marFont;
        int strWidth = 0;
        Brush white = Brushes.White;

        int rotate = 0;
        void drawMarquee() {
            Graphics g = marqueeGraphics;
            g.Clear(Color.Transparent);
            float w = conMarquee.Width;
            float h = conMarquee.Height;

            marqueePos--;
            g.DrawString(current, marFont, white, marqueePos, 0);

            if (marqueePos < -strWidth) {
                read(g);
                marqueePos = (int)w;
            }
            
        }
        


        void read(Graphics g) {
            lock (marqueeContents)
            {
                if (marqueeContents.Count > 0)
                {
                    marqueePtr++; if (marqueePtr >= marqueeContents.Count) { marqueePtr = 0; }
                    current = marqueeContents[marqueePtr];
                }
            }
            strWidth =(int) g.MeasureString(current, marFont).Width;
        }
        WebClient wc = new WebClient() {
            Encoding=Encoding.UTF8
        };
        private void newsGetter_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //HttpWebRequest xhr = BomberUtils.MakeHttpGet(Program.urlnews, null);
                //xhr.Referer = "https://3g.dxy.cn/newh5/view/pneumonia_timeline";
                //xhr.Accept = "application/json";
                //xhr.ContentType = "application/json;charset=utf-8";
                //String newshtml = BomberUtils.GetHttpResponse(xhr);
                
                string newshtml = wc.DownloadString(Program.urlnews);
                String html = wc.DownloadString(Program.url);//Utils.httpGet(Program.url);


                String json1 = Utils.SearchJson(newshtml, "\"data\":");
                String json0 = Utils.SearchJson(html, "window.getTimelineService");
                status = null;
                status = Utils.SearchJson(html, "window.getStatisticsService");
                status = JsonConvert.DeserializeObject<SummaryItem>(status).countRemark;
                status = status.Replace('\r', ' ').Replace('\n', ' ');
                List<NewsItem> list0 = JsonConvert.DeserializeObject<List<NewsItem>>(json0);
                list0.AddRange(JsonConvert.DeserializeObject<List<NewsItem>>(json1));
                e.Result = list0;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        int lastlen = -1;

        string pstatus = "预计需要更长时间获取状态...", status = null;
        private void newsGetter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<NewsItem> news = e.Result as List<NewsItem>;
            if (null != status) { pstatus = status; }
            if (null != news)
            {
                newsItems = news;
                frmNewsList.lblInfoArea.Text = "于"+ DateTime.Now.ToString("yyyy\\-MM\\-dd HH\\:mm\\:ss")+"更新";
            }
            
            if (null != newsItems) { 
                lock (marqueeContents)
                {
                    marqueeContents.Clear();
                    marqueeContents.Add(newsItems.First().title);
                    marqueeContents.Add(pstatus);
                    //read(marqueeGraphics);
                }

                if (lastlen != -1)
                {
                    int count = news.Count - lastlen;
                    if (count > 0 && chkNotification.Checked) {
                        for(int i = count - 1; i >= 0; i--)
                        {
                            NewsItem ni = news[i];
                            notificationQueue.Add(new FrmNewsDialog(ni.title,ni.summary,ni.sourceUrl));
                        }
                        raiseNotification();
                    }
                }
                lastlen = news.Count;
            }
            
            else
            {
                marqueeContents.Clear();
                marqueeContents.Add("预计需要更长时间获取新闻。");
                updateTimer.Interval = 30000;
            }
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            updateTimer.Interval = 8 * 60000;
            if (!newsGetter.IsBusy) {
                newsGetter.RunWorkerAsync();
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateTimer_Tick(null, null);
        }

        private void btnExpand_Click(object sender, EventArgs e)
        {
            frmNewsList.Visible = !frmNewsList.Visible;
            rotate = frmNewsList.Visible ? 180 : 0;
        }

        List<FrmNewsDialog> notificationQueue = new List<FrmNewsDialog>();
        bool isDialogShowing = false;
        void raiseNotification() {
            if (isDialogShowing) { return; }
            while (notificationQueue.Count > 0) {
                FrmNewsDialog fd = notificationQueue[0];
                notificationQueue.RemoveAt(0);
                isDialogShowing = true;
                fd.ShowDialog();
                isDialogShowing = false;
            }
        }

        private void toolStripMenuItem1_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = toolStripMenuItem1.Checked;
        }

        private void splashTimer_Tick(object sender, EventArgs e)
        {
            splashTimer.Enabled = false;

            notificationQueue.Add(new FrmNewsDialog("欢迎使用 疫情播报桌面小部件","数据来自丁香园（爬虫），每8分钟更新一次。获取新闻可能失败（玄学问题），请换网或在github提Issue。\r\n点击详情打开github页面", "https://github.com/ZYFDroid/PlagueInc-Style-News"));
            notificationQueue.Add(new FrmNewsDialog("提示：在标题栏左边图标右击有选项菜单","右击标题栏左边NEWS图标，可以打开选项菜单，可以设置通知，置顶，刷新以及退出。", "https://github.com/ZYFDroid/PlagueInc-Style-News"));
            notificationQueue.Add(new FrmNewsDialog("提示：单击新闻列表项目打开详情","单击新闻列表中的一项可以查看标题，内容和原始链接。", "https://github.com/ZYFDroid/PlagueInc-Style-News"));
            raiseNotification();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void 更新ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            updateTimer_Tick(sender, e);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TopMost = toolStripMenuItem1.Checked;
        }
    }
}
