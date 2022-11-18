using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Weibo.Models
{
    public class BlogItem
    {
        public string created_at { get; set; }
        public long id { get; set; }
        public string idstr { get; set; }
        public string mblogid { get; set; }
        public UserInfo user { get; set; }
        public string text_raw { get; set; }
        public string text { get; set; }
        public PageInfo page_info { get; set; }
        public string[] pic_ids { get; set; }
        public int pic_num { get; set; }
        public bool isLongText { get; set; }

        public string region_name { get; set; }
        //转发详细
        public BlogItem retweeted_status { get; set; }
    }
}
