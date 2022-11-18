using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcessAssistant.Configuration;
using ProcessAssistant.File;
using ProcessAssistant.History;
using ProcessAssistant.Logger;
using ProcessAssistant.State;
using ProcessAssistant.TaskModel;
using ProcessAssistant.Weibo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcessAssistant.Weibo
{
    public class WeiboAssistant : IProcessAssistant
    {
        //private string[] _ajaxRequestUrl { get; set; } = new[]
        //{
        //        @"https:\/\/weibo.com\/ajax\/profile\/info\?uid=[0-9]+",//用户主页信息
        //        @"https:\/\/weibo.com\/ajax\/statuses\/mymblog\?uid=[0-9]+",//用户博文
        //        @"https:\/\/weibo.com\/ajax\/friendships\/friends\?page=[0-9]+&uid=[0-9]+",//关注列表
        //        @"https:\/\/weibo.com\/ajax\/friendships\/friends\?relate=fans&page=[0-9]+&uid=[0-9]+",//粉丝列表
        //        @"https:\/\/weibo.com\/ajax\/profile\/detail\?uid=[0-9]+",//用户详细信息
        //        @"https:\/\/weibo.com\/ajax\/statuses\/longtext\?id="//长博文文本
        //};



        private UserInfo _homePage;
        private PluginConfiguration _config;
        private List<string> _createdTaskUrls;
        private Random _randomer = new Random();
        private ILogger _logger;
        private Dictionary<string, object> _localStorageReference;

        private static HistoryManager<string> _historyManager { get; set; }
        private IFileService _fileService;
        private StateService _stateService { get; set; }

        private int _blogsLoopQueryTimes = 0;
        private int _followsLoopQueryTimes = 0;
        private bool _updateMode = false;

        public WeiboAssistant()
        {
            _stateService = new StateService();
            _fileService = new FileService();

            _createdTaskUrls = new List<string>();
            var cf = new PluginConfigurationService();
            _config = cf.GetPluginConfig() as Weibo.PluginConfiguration;

        }

        #region Private Methods

        /// <summary>
        /// 构建博文中的图片下载地址
        /// </summary>
        /// <param name="fileIdStr"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        private string GetImageFileUrl(string fileIdStr, string fileType, int index)
        {
            fileType = fileType.Trim('.');
            return string.Format(_config.BlogImageTemplate[index], _randomer.Next(1, 5), fileIdStr, fileType);
        }

        /// <summary>
        /// 获取存储路径，并创建不存在的文件夹
        /// </summary>
        /// <param name="storagetDescriptor"></param>
        /// <returns></returns>
        private string GetSavePath(ResultStorageDescriptor storagetDescriptor)
        {
            var folder = Path.Combine(_config.WorkSpaceRoot, storagetDescriptor.SubPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return Path.Combine(folder, $"{storagetDescriptor.Name}{storagetDescriptor.FileType}");
        }

        /// <summary>
        /// 微博总数教上次获取是否有变化
        /// </summary>
        /// <returns></returns>
        private bool IsHasUpdate()
        {
            //更新模式下直接返回有更新
            if (_updateMode)
                return true;

            var homeId = _homePage is null ? _localStorageReference["Id"] : _homePage.idstr;
            var path = GetSavePath(
                 new ResultStorageDescriptor
                 {
                     FileType = ".json",
                     Name = "UserHomePage",
                     SubPath = $"{homeId}/Original/"
                 });
            if (!_fileService.IsFileExists(path))
            {
                return true;
            }
            var oldInfo = _fileService.LoadFromFile<UserInfo>(path);
            return _homePage?.statuses_count != oldInfo?.statuses_count;
        }
        private string GetFileNameFromUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var realUrl))
            {
                return Path.GetFileName(realUrl.LocalPath);
            }
            else
            {
                var mainUrl = url.Split("?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim('/');
                return Path.GetFileName(mainUrl);
            }
        }

        private bool IsAlreadyDone(long id)
        {
            return _historyManager.IsInHistory(id.ToString());

        }
        private bool IsAlreadyDone(string idstr)
        {
            return _historyManager.IsInHistory(idstr);
        }
        #endregion

        #region AjaxDataProcess


        private void LongBlogContentDataProcess(JObject jobj, string sourceUrl)
        {
            try
            {
                var longText = jobj.SelectToken("data.longTextContent").ToString();
                //var uuid = jobj.SelectToken("data.topic_struct.actionlog.uuid").Value<string>();
                var extData = jobj.SelectToken("data.topic_struct.actionlog.ext")?.ToString();
                if (string.IsNullOrWhiteSpace(extData))
                {
                    extData = jobj.SelectToken("data.url_struct[0].actionlog.ext")?.ToString();
                }
                if (string.IsNullOrWhiteSpace(extData))
                {
                    extData = jobj.SelectToken("data.topic_struct[0].actionlog.ext")?.ToString();
                }

                var blogId = string.Empty;

                if (string.IsNullOrWhiteSpace(extData))
                {
                    if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out var realUrl))
                    {
                        blogId = $"0000000/{realUrl.Query.Replace("?id=", "")}";
                    }
                    else
                    {
                        blogId = $"0000000/{DateTime.Now.Ticks.ToString()}";
                    }
                }
                else
                {
                    blogId = extData.Split('|').FirstOrDefault(i => i.StartsWith("mid:"))?.Substring(4);
                }
                var homeId = _homePage is null ? _localStorageReference["Id"] : _homePage.idstr;
                //写入文档
                OnSaveData(
                    Symbols.BlogTextContext,
                    longText,
                    new ResultStorageDescriptor
                    {
                        FileType = ".txt",
                        Name = "context_long",
                        SubPath = $"{homeId}/{blogId}"
                    });
            }
            catch
            {
                Logger.Logger.DefaultLogger?.LogDebug("处理长文本异常：{0}", jobj.ToString());
            }
        }

        private void UserInfoDetailProcess(JObject jobj)
        {
            var userObj = jobj.SelectToken("data").ToString();
            var userDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDetailInfo>(userObj);

            var homeId = _homePage is null ? _localStorageReference["Id"] : _homePage.idstr;
            //写入文档
            OnSaveData(
                   Symbols.UserInfo,
                   JsonConvert.SerializeObject(userDetail),
                   new ResultStorageDescriptor
                   {
                       FileType = ".json",
                       Name = "UserInfo",
                       SubPath = $"{homeId}"
                   });
            OnSaveData(
                Symbols.UserInfo,
                userObj,
                new ResultStorageDescriptor
                {
                    FileType = ".json",
                    Name = $"{DateTime.Now:yyyy年MM月dd日HH时mm分ss秒fff}",
                    SubPath = $"{homeId}/Original/Userinfo"
                });
        }

        private void FansAndFansollowsProcess(JObject jobj)
        {
            var total = jobj.SelectToken("total_number").ToString();
            var fans = jobj["users"].ToString();
            var fanList = JsonConvert.DeserializeObject<List<UserInfo>>(fans);
            //jobj.SelectTokens("users[*].profile_url");
            bool anyNew = false;
            fanList.All(f =>
            {
                if (_config.DenyUrls.Any(d => f.verified_reason.IndexOf(d) > -1 | f.screen_name.IndexOf(d) > -1 | f.location.IndexOf(d) > -1))
                {
                    Logger.Logger.DefaultLogger?.LogDebug($"Follow Or Fan :{f.screen_name} 被排除。{f.verified_reason}，{f.location}");
                    return true;
                }
                var url = f.profile_url;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var newUrl = string.Format("https://weibo.com/{0}", url.TrimStart('/'));
                    var idStr = AnalysisStartUrl(newUrl);
                    if (!IsAlreadyDone(idStr))
                    {
                        _createdTaskUrls.Add(newUrl);
                        anyNew = true;
                        _followsLoopQueryTimes = 0;

                    }

                }
                return true;
            });
            //更新模式下直接标记有更新
            if (_updateMode)
                anyNew = true;

            Logger.Logger.DefaultLogger?.LogDebug("改变{0}的循环标记为：{1}", Symbols.FollowsOrFansCountChanged, anyNew);
            _stateService.Set<bool>(Symbols.FollowsOrFansCountChanged, anyNew);
        }

        private void BlogsProcess(JObject jobj)
        {
            var listData = jobj.SelectToken("data.list").ToString();
            var blogs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BlogItem>>(listData);
            var anyNew = false;
            blogs.ForEach(item =>
            {
                if (_BlogSubProcess(item))
                {
                    anyNew = true;
                    _blogsLoopQueryTimes = 0;
                }
            });
            //更新模式下直接标记有更新
            if (_updateMode)
                anyNew = true;

            var homeId = _homePage is null ? _localStorageReference["Id"] : _homePage.idstr;
            OnSaveData(
                Symbols.BlogsListData,
                listData,
                new ResultStorageDescriptor
                {
                    FileType = ".json",
                    Name = $"{DateTime.Now:yyyy年MM月dd日HH时mm分ss秒fff}",
                    SubPath = $"{homeId}/Original/BlogList"
                });
            Logger.Logger.DefaultLogger?.LogDebug("改变{0}的循环标记为：{1}", Symbols.BlogCountChanged, anyNew);
            _stateService.Set(Symbols.BlogCountChanged, anyNew);

        }

        private void HomePageProcess(JObject jobj)
        {
            var hoePageData = jobj.SelectToken("data.user").ToString();
            _homePage = JsonConvert.DeserializeObject<UserInfo>(hoePageData);
            var totalBlogsCountChanged = IsHasUpdate();
            Logger.Logger.DefaultLogger?.LogDebug("变更{0}的状态为{1}", Symbols.BlogCountChanged, totalBlogsCountChanged);
            _stateService.Set(Symbols.BlogCountChanged, totalBlogsCountChanged);

            //保存文档
            OnSaveData(
                Symbols.UserHomePage,
                hoePageData,
                new ResultStorageDescriptor
                {
                    FileType = ".json",
                    Name = "UserHomePage",
                    SubPath = $"{_homePage.idstr}/Original/"
                });

            var iconUrl = GetFileNameFromUrl(_homePage.avatar_large);
            //下载头像图片
            OnDownloadFile(
                Symbols.BlogImageFile,
                Path.GetFileNameWithoutExtension(iconUrl),
                new ResultStorageDescriptor
                {
                    FileType = Path.GetExtension(iconUrl),
                    Name = Path.GetFileNameWithoutExtension(iconUrl),
                    SubPath = $"{_homePage.id}/Icons"
                });

            _homePage.cover_image_phone?.Split(';').ToList().ForEach(fileUrl =>
            {
                //下载横幅图片
                OnDownloadFile(
                    Symbols.CoverImageFile,
                    fileUrl,
                    new ResultStorageDescriptor
                    {
                        FileType = Path.GetExtension(fileUrl),
                        Name = Path.GetFileNameWithoutExtension(fileUrl),
                        SubPath = $"{_homePage.id}/Covers"
                    });
            });
        }


        private bool _BlogSubProcess(BlogItem item)
        {
            if (_historyManager.IsInHistory(item.idstr))
            {
                return false;
            }
            else
            {
                if (item.retweeted_status != null)
                {
                    _BlogSubProcess(item.retweeted_status);
                }
                if (item.user == null)
                {
                    Logger.Logger.DefaultLogger?.LogDebug("微博已被删除 {0}", item.id);
                }
                else
                {
                    if (item.page_info != null)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(item.page_info.page_pic);
                        var fileExt = Path.GetExtension(item.page_info.page_pic);
                        try
                        {

                            OnDownloadFile(
                                    Symbols.BlogImageFile,
                                    fileName,
                                    new ResultStorageDescriptor
                                    {
                                        FileType = fileExt,
                                        Name = fileName,
                                        SubPath = $"{item.user.idstr}/{item.id}/Images"
                                    });
                        }
                        catch
                        {
                            Logger.Logger.DefaultLogger.LogDebug("下载页面封面失败：{0}", item.page_info.page_title);
                        }
                        OnSaveData(
                                Symbols.BlogTextContext,
                                JsonConvert.SerializeObject(item.page_info),
                                new ResultStorageDescriptor
                                {
                                    FileType = ".txt",
                                    Name = $"context_{item.page_info.object_type}",
                                    SubPath = $"{item.user.idstr}/{item.id}"
                                });
                    }
                    if (item.pic_num > 0 && item.pic_ids != null)
                    {
                        for (int index = 0; index < item.pic_num; index++)
                        {
                            OnDownloadFile(
                                Symbols.BlogImageFile,
                                item.pic_ids[index],
                                new ResultStorageDescriptor
                                {
                                    FileType = ".jpg",
                                    Name = item.pic_ids[index],
                                    SubPath = $"{item.user.idstr}/{item.id}/Images"
                                });
                        }
                    }
                    OnSaveData(
                        Symbols.BlogTextContext,
                        item.text,
                        new ResultStorageDescriptor
                        {
                            FileType = ".txt",
                            Name = "context",
                            SubPath = $"{item.user.idstr}/{item.id}"
                        });
                }

                _historyManager.AddToHistory(item.id.ToString());
                return true;
            }
        }

        private string AnalysisStartUrl(string taskUrl)
        {
            if (Regex.IsMatch(taskUrl, "https://weibo.com/u/[0-9]+"))
            {
                return taskUrl.Replace("https://weibo.com/u/", "").Trim('/');
            }
            else if (Regex.IsMatch(taskUrl, "https://weibo.com/[0-9]+"))
            {
                return taskUrl.Replace("https://weibo.com/", "").Trim('/');
            }

            return "";
        }

        #endregion


        public string[] GetAjaxUrlFilters()
        {
            return _config.AjaxResposeUrls.Keys.ToArray();
        }

        public string[] GetDataUrlFilters()
        {
            return _config.DataResposeUrls;
        }


        public void OnTaskStart(
            string taskUrl,
            Dictionary<string, object> cacheStorage,
            object parameters,
            HistoryManager<string> historyManager,
            ILogger logger)
        {
            _logger = logger;
            _historyManager = historyManager;

            _localStorageReference = cacheStorage;
            _localStorageReference["WorkSpaceRoot"] = _config.WorkSpaceRoot;

            var IdStr = AnalysisStartUrl(taskUrl);
            _localStorageReference["Id"] = IdStr;

            _stateService.Set(Symbols.BlogCountChanged, true);
            _stateService.Set(Symbols.FollowsOrFansCountChanged, true);
            _blogsLoopQueryTimes = 0;
            _followsLoopQueryTimes = 0;

            //CheckUpdate("");
        }

        public void OnTaskFinished()
        {

        }

        public void OnTaskCancelled()
        {

        }

        public Task OnAjaxDataProcess(string symbol, string sourceUrl, MemoryStream ajaxDataStream)
        {
            try
            {
                ajaxDataStream.Position = 0;
                var ajaxData = Encoding.UTF8.GetString(ajaxDataStream.ToArray());
                var jobj = JObject.Parse(ajaxData);

                switch (symbol)
                {
                    case Symbols.UserHomePageAjax:
                        HomePageProcess(jobj);
                        break;
                    case Symbols.UserBlogsAjax:
                        BlogsProcess(jobj);
                        break;
                    case Symbols.UserFollowsAjax:
                    case Symbols.UserFansAjax:
                        FansAndFansollowsProcess(jobj);
                        break;
                    case Symbols.UserInfoAjax:
                        UserInfoDetailProcess(jobj);
                        break;
                    case Symbols.UserLongBlogContentAjax:
                        LongBlogContentDataProcess(jobj, sourceUrl);

                        break;
                    default:
                        break;
                }

                return Task.CompletedTask;
            }
            catch (Exception error)
            {
                Logger.Logger.DefaultLogger?.LogError(error, "Ajax Plugin Error");
                throw;
            }

        }

        public void OnSaveData(string symbol, string data, ResultStorageDescriptor storagetDescriptor)
        {
            string savePath = GetSavePath(storagetDescriptor);
            if (_fileService.IsFileExists(savePath))
            {
                var f = new FileInfo(savePath);
                if (f.Length == data.Length)
                {
                    _logger.LogDebug("文件[{0}]已存在,且大小相同，已跳过", savePath);
                    return;
                }
            }
            _fileService.SaveStringDataToFile(data, savePath);
            _logger.LogDebug("文件[{0}]已保存", savePath);
        }

        public void OnDownloadFile(string symbol, string data, ResultStorageDescriptor storagetDescriptor)
        {
            try
            {
                var savePath = GetSavePath(storagetDescriptor);

                switch (symbol)
                {
                    case Symbols.BlogImageFile:
                        if (_fileService.IsFileExists(savePath))
                        {
                            var fileTarget = new FileInfo(savePath);
                            if (fileTarget.Length > 0)
                            {
                                _logger?.LogDebug("图片[{0}]的下载已取消，因为目标文件[{1}]已存在，大小为[{2}]KB",
                                    data, savePath, fileTarget.Length / 1024);
                                return;
                            }
                        }

                        for (int i = 0; i < _config.BlogImageTemplate.Count; i++)
                        {
                            var url = GetImageFileUrl(data, storagetDescriptor.FileType, i);
                            _fileService.DownloadFileFromRemote(url, savePath, i.ToString());
                            _logger.LogDebug("文件[{0}]已保存", url);
                        }

                        break;
                    default:
                        _fileService.DownloadFileFromRemote(data, savePath);
                        break;
                }

            }
            catch(Exception e)
            {
                _logger.LogError(e,"下载文件[{0}]失败", data);
            }
        }

        public void OnDataCached(string symbol, string data)
        {

        }

        public void OnSaveCachedData(string symbol, ResultStorageDescriptor storagetDescriptor)
        {

        }

        public bool CanProcessAjaxData(string url, out string symbol)
        {
            var result = _config.AjaxResposeUrls.FirstOrDefault(s => Regex.IsMatch(url, s.Key));
            symbol = result.Value;
            return !string.IsNullOrWhiteSpace(result.Key);
        }

        public bool CanLoopBreak(string symbol)
        {
            switch (symbol)
            {
                case Symbols.BlogCountChanged:
                    _blogsLoopQueryTimes++;
                    if (_blogsLoopQueryTimes > _config.MaxExitLoopQueryTimes)
                        return true;
                    return !_stateService.Get<bool>(Symbols.BlogCountChanged);
                case Symbols.FollowsOrFansCountChanged:
                    _followsLoopQueryTimes++;
                    if (_followsLoopQueryTimes > _config.MaxExitLoopQueryTimes)
                        return true;
                    return !_stateService.Get<bool>(Symbols.FollowsOrFansCountChanged);
                default:
                    break;
            }
            return false;
        }

        public IEnumerable<string> OnNewTaskCreated(out object parameters)
        {
            if (_createdTaskUrls == null || _createdTaskUrls.Count == 0)
            {
                parameters = null;
                return null;
            }
            else
            {
                var data = _createdTaskUrls.ToArray();
                _createdTaskUrls.Clear();

                var result = data.ToArray();
                parameters = null;
                return result;
            }
        }

        public void PreviewDownloadFile(string symbol, string data, ResultStorageDescriptor storagetDescriptor)
        {

        }

        public void PreviewSaveData(string symbol, string sourceUrl, string data, ResultStorageDescriptor storagetDescriptor)
        {

        }

        public IEnumerable<string> RebuildHistory(string symbol, HistoryManager<string> historyManager)
        {
            var result = new List<string>();
            //将本地已抓取的用户主页url重新放入任务列表
            foreach (var item in Directory.GetDirectories(_config.WorkSpaceRoot))
            {
                var userFolderName = Path.GetFileName(item);
                //是weibo抓取的目录
                if (long.TryParse(userFolderName, out _))
                {

                    //获取用户主页
                    var userInfoFile = Path.Combine(item, "Original", "UserHomePage.json");
                    if (_fileService.IsFileExists(userInfoFile))
                    {
                        var data = _fileService.LoadFromFile<UserInfo>(userInfoFile);
                        result.Add(string.Format("https://weibo.com/{0}", data.profile_url.TrimStart('/')));
                    }
                    else
                    {
                        result.Add(string.Format("https://weibo.com/u/{0}", userFolderName));
                    }

                    //微博发布后基本不会变(除非下载的文件大小为零)，直接添加到历史记录中排除，只更新新加的
                    foreach (var blogs in Directory.GetDirectories(item))
                    {
                        var blogId = Path.GetFileName(blogs);
                        if (long.TryParse(blogId, out var id))
                        {
                            if (Directory.GetFiles(blogs).Any(
                                (p) =>
                                {
                                    try
                                    {
                                        return new FileInfo(p).Length == 0;
                                    }
                                    catch
                                    {
                                        return true;
                                    }
                                }))
                            {

                            }
                            else
                            {
                                historyManager.AddToHistory(blogId);
                            }
                        }
                    }
                }
            }
            return result;

        }

        public void SwitchUpdateMode(string symbol, bool updateMode)
        {
            _updateMode = updateMode;
        }
    }
}
