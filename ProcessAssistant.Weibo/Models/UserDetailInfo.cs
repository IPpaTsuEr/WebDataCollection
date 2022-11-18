using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Weibo.Models
{
    public class EducationInfo
    {
        public string school { get; set; }
    }

    public class UserDetailInfo
    {
        public EducationInfo education { get; set; }
        //星座
        public string birthday { get; set; }
        public DateTime created_at { get; set; }
        public string description { get; set; }
        public string gender { get; set; }
        public string ip_location { get; set; }
        public string desc_text { get; set; }
    }
}
