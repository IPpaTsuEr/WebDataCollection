using Newtonsoft.Json;
using ProcessAssistant.File;
using ProcessAssistant.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Configuration
{
    public class AppConfigurationService : IAppConfigurationService
    {
        private readonly string AppConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "appConfig.json");
        private readonly IFileService _fileService;
        private Dictionary<string, TaskExcuteConfiguration> _map;

        public AppConfiguration ApplacationConfiguration { get; set; }

        public AppConfigurationService(IFileService fileService)
        {
            _map = new Dictionary<string, TaskExcuteConfiguration>();

            _fileService = fileService;

            ApplacationConfiguration = _fileService.LoadFromFile<AppConfiguration>(AppConfigurationPath);
            var localPath = ApplacationConfiguration.TaskConfigurationStoragePath;

            if (Directory.Exists(localPath))
            {
                Directory.GetFiles(localPath).ToList().ForEach(i =>
                {
                    try
                    {
                        var taskConfig = _fileService.LoadFromFile<TaskExcuteConfiguration>(i);
                        _map.Add(taskConfig.MatchRegex, taskConfig);
                    }
                    catch
                    {
                        var taskConfigs = _fileService.LoadFromFile<List<TaskExcuteConfiguration>>(i);
                        taskConfigs.ForEach(cfg =>
                        {
                            _map.Add(cfg.MatchRegex, cfg);
                        });
                    }
                });
            }

        }


        public TaskExcuteConfiguration GetTaskConfiguration(string url)
        {
            var key = _map.Keys.ToList().Find(i => Regex.IsMatch(url, i));
            if (string.IsNullOrWhiteSpace(key))
            {
                Logger.DefaultLogger.LogDebug("无法找到与[{0}]匹配的解析脚本",url);
                return null;
            }
            if (_map.TryGetValue(key, out var taskConfiguration))
            {
                return taskConfiguration;
            }
            return null;
        }
        public TaskExcuteConfiguration GetTaskConfigurationByName(string name)
        {
            var key = _map.Values.FirstOrDefault(i => i.Name == name);
            if (key == null)
            {
                Logger.DefaultLogger.LogDebug("无法找到与[{0}]名称匹配的解析脚本", name);
                return null;
            }
            
            return key;
        }
    }
}
