using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Weibo.Models
{
    public class UserInfo
    {
        public long id { get; set; }
        public string idstr { get; set; }
        //简介
        public string description { get; set; }
        //昵称
        public string screen_name { get; set; }
        //头像地址
        public string profile_image_url { get; set; }
        //头像大图
        public string avatar_large { get; set; }
        //blog地址
        public string profile_url { get; set; }
        //关注我
        public bool follow_me { get; set; }
        //已关注
        public bool following { get; set; }
        //认证信息
        public string verified_reason { get; set; }
        //地址
        public string location { get; set; }

        //封面
        public string cover_image_phone { get; set; }
        //关注个数
        public long statuses_count { get; set; }
    }
}
