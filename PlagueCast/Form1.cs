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
        //WebClient wc = new WebClient() {Encoding=Encoding.UTF8 };
        private void newsGetter_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {


                string newshtml = Utils.httpGet(Program.urlnews);//wc.DownloadString(Program.urlnews);
                String json1 = Utils.SearchJson(newshtml, "\"results\"");
                List<NewsItem> list0 = JsonConvert.DeserializeObject<List<NewsItem>>(json1);
                e.Result = list0;
                
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                
                String allhtml = Utils.httpGet(Program.urloverall);// wc.DownloadString(Program.url);//Utils.httpGet(Program.url);
                status = null;
                status = Utils.SearchJson(allhtml, "\"results\"");
                SummaryItem si = JsonConvert.DeserializeObject<List<SummaryItem>>(status)[0];
                status = $"截至{Utils.parstUnixTime(si.updateTime).ToString("yyyy\\年MM\\月dd\\日 HH\\:mm")}，全国确诊{si.confirmedCount}例，疑似{si.suspectedCount}例，治愈{si.curedCount}例，死亡{si.deadCount}例。";


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        long newestTime = -1;

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
            
            if (null != newsItems && newsItems.Count>0) { 
                lock (marqueeContents)
                {
                    marqueeContents.Clear();
                    marqueeContents.Add(newsItems.First().title);
                    marqueeContents.Add(pstatus);
                    //read(marqueeGraphics);
                }

                if (newestTime != -1)
                {
                    notificationQueue.AddRange(newsItems.Where(ni => ni.pubDate > newestTime).Select(p => new FrmNewsDialog(p.title,p.summary,p.sourceUrl)));
                    raiseNotification();
                }
                newestTime = newsItems.First().pubDate;
            }
            
            else
            {
                marqueeContents.Clear();
                marqueeContents.Add("预计需要更长时间获取新闻。");
                updateTimer.Interval = 60000;
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
            notificationQueue.Distinct();
            if (isDialogShowing) { return; }
            while (notificationQueue.Count > 0) {
                FrmNewsDialog fd = notificationQueue[0];
                isDialogShowing = true;
                fd.ShowDialog();
                notificationQueue.RemoveAt(0);
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

            notificationQueue.Add(new FrmNewsDialog("欢迎使用 疫情播报桌面小部件", "数据来自丁香园（爬虫由 http://lab.isaaclin.cn/nCoV/ 提供。)，每8分钟更新一次。\r\n点击详情打开丁香园页面", Program.url));
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

        private void 打开丁香园疫情播报页面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationQueue.Add(new FrmNewsDialog("提示: 数据来自丁香园", "数据来自丁香园（爬虫由 http://lab.isaaclin.cn/nCoV/ 提供。)，每8分钟更新一次。\r\n点击详情打开丁香园疫情播报页面", Program.url));
            raiseNotification();
        }

        private void 打开Github页面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationQueue.Add(new FrmNewsDialog("提示: 打开Github页面查看更新", "如果新闻持续不更新，或者显示奇怪的内容，请检测有没有版本更新。\r\n点击详情打开Github页面", "https://github.com/ZYFDroid/PlagueInc-Style-News"));
            raiseNotification();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TopMost = toolStripMenuItem1.Checked;
        }
    }
}
