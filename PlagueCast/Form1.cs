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

        private void newsGetter_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                String html = Utils.httpGet(Program.url);
                String json = Utils.SearchJson(html, "window.getTimelineService");
                status = Utils.SearchJson(html, "window.getStatisticsService");
                status = JsonConvert.DeserializeObject<SummaryItem>(status).countRemark;
                status = status.Replace('\r', ' ').Replace('\n', ' ');
                e.Result = JsonConvert.DeserializeObject<List<NewsItem>>(json);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        int lastlen = -1;

        string status = "";
        private void newsGetter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<NewsItem> news = e.Result as List<NewsItem>;
            
            if (null != news)
            {
                newsItems = news;
                lock (marqueeContents)
                {
                    marqueeContents.Clear();
                    marqueeContents.Add(news.First().title);
                    marqueeContents.Add(status);
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
                marqueeContents.Add("获取新闻失败!");
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
    }
}
