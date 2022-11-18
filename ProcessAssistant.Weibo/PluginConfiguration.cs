using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Weibo
{
     public class PluginConfiguration : ProcessAssistant.Configuration.PluginConfiguration
    {
        public Dictionary<string,string> AjaxResposeUrls { get; set; }
        public string[] DataResposeUrls { get; set; }
        public List<string> BlogImageTemplate { get; set; }
        public int MaxExitLoopQueryTimes { get; set; }
        public List<string> DenyUrls { get; set; }
    }
}
