using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagueCast
{
    public class NewsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long pubDate { get; set; }
        /// <summary>
        /// 41分钟前
        /// </summary>
        public string pubDateStr { get; set; }
        /// <summary>
        /// 陕西新增 7 例确诊病例，累计全省 22 例
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 全省累计报告确诊病例22例】截至1月26日10时，陕西新增7例新型冠状病毒感染的肺炎确诊病例；新增密切接触者94例；全省累计报告确诊病例22例，密切接触者273例，均集中医学观察。
        /// </summary>
        public string summary { get; set; }
        /// <summary>
        /// 人民日报
        /// </summary>
        public string infoSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sourceUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string provinceId { get; set; }
        /// <summary>
        /// 陕西省
        /// </summary>
        public string provinceName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long createTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long modifyTime { get; set; }
    }

    public class SummaryItem
    {
        /// <summary>
        /// 
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long createTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long modifyTime { get; set; }
        /// <summary>
        /// 野生动物，可能为中华菊头蝠
        /// </summary>
        public string infectSource { get; set; }
        /// <summary>
        /// 未完全掌握，存在人传人、医务人员感染、一定范围社区传播
        /// </summary>
        public string passWay { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string imgUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string dailyPic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string summary { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string deleted { get; set; }
        /// <summary>
        /// 全国 确诊 2002 例 疑似 2684 例
        ///死亡 56 例 治愈 49 例
        /// </summary>
        public string countRemark { get; set; }
        /// <summary>
        /// 新型冠状病毒 2019-nCoV
        /// </summary>
        public string virus { get; set; }
        /// <summary>
        /// 病毒是否变异：存在可能
        /// </summary>
        public string remark1 { get; set; }
        /// <summary>
        /// 疫情是否扩散：是
        /// </summary>
        public string remark2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string remark3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string remark4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string remark5 { get; set; }
        /// <summary>
        /// 疑似病例数来自国家卫健委数据，目前为全国数据，未分省市自治区等
        /// </summary>
        public string generalRemark { get; set; }
    }
}
