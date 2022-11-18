using ProcessAssistant.Configuration;
using ProcessAssistant.Weibo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Weibo
{
    public class PluginConfigurationService : IPluginConfigurationService
    {
        private readonly IConfigurationService _configurationService = null;
        private PluginConfiguration _weiboConfiguration = null;
        private string _configPath = Path.Combine(Path.GetDirectoryName(typeof(WeiboAssistant).Assembly.Location), $"{typeof(WeiboAssistant).Assembly.GetName().Name}.json");

        public PluginConfigurationService()
        {
            if (_configurationService == null)
            {
                _configurationService = new ConfigurationService();
            }
        }
        public PluginConfigurationService(IConfigurationService configurationService = null)
        {
            _configurationService = configurationService;
            if (_configurationService == null)
            {
                _configurationService = new ConfigurationService();
            }
        }
        public ProcessAssistant.Configuration.PluginConfiguration GetPluginConfig()
        {
            if (_weiboConfiguration is null)
            {
                try
                {
                    _weiboConfiguration = _configurationService.Load<PluginConfiguration>(_configPath);
                }
                catch
                {
                    CreatDefualtConfiguration();
                    _configurationService.Save(_configPath, _weiboConfiguration);
                }

            }
            return _weiboConfiguration;
        }

        public void CreatDefualtConfiguration()
        {
            _weiboConfiguration = new PluginConfiguration
            {
                DisplayName = "WeiBo",
                SymbolName = typeof(WeiboAssistant).FullName,
                MatchedHostName = "weibo.com",
                WorkSpaceRoot = $"{Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), "WeiboCollection")}",
                Version = typeof(WeiboAssistant).Assembly.GetName().Version,
                
                BlogImageTemplate = new List<string> { 
                    "https://wx{0}.sinaimg.cn/oslarge/{1}.{2}",
                    "https://wx{0}.sinaimg.cn/large/{1}.{2}"
                },
                AjaxResposeUrls = new Dictionary<string, string>{
                    { @"https:\/\/weibo.com\/ajax\/profile\/info\?uid=[0-9]+", Symbols.UserHomePageAjax},//用户主页信息
                    { @"https:\/\/weibo.com\/ajax\/statuses\/mymblog\?uid=[0-9]+",Symbols.UserBlogsAjax},//用户博文
                    { @"https:\/\/weibo.com\/ajax\/friendships\/friends\?page=[0-9]+&uid=[0-9]+",Symbols.UserFollowsAjax},//关注列表
                    { @"https:\/\/weibo.com\/ajax\/friendships\/friends\?relate=fans&page=[0-9]+&uid=[0-9]+",Symbols.UserFansAjax},//粉丝列表
                    { @"https:\/\/weibo.com\/ajax\/profile\/detail\?uid=[0-9]+",Symbols.UserInfoAjax},//用户详细信息
                    { @"https:\/\/weibo.com\/ajax\/statuses\/longtext\?id=",Symbols.UserLongBlogContentAjax},//长博文文本
                 }
            };
        }
    }
}
