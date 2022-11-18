using ProcessAssistant;
using ProcessAssistant.Configuration;
using ProcessAssistant.File;
using ProcessAssistant.History;
using ProcessAssistant.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataCollection.Interface;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Services.CollectionTask
{
    public class CollectionTaskService : Interface.CollctionTask.ITaskService
    {
        private object _locker = new object();
        private CollectionTaskExcuteService _currentTask { get; set; }
        private HistoryManager<string> _historyManager { get; }
        private bool _canExcute { get; set; }
        private int _maxExcute { get; set; } = 1;
        private Dictionary<string, object> _taskList { get; set; }
        private Dictionary<CollectionTaskExcuteService, CancellationTokenSource> _excutingList { get; set; }
        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        //private IProcessAssistant pluginProcessAssistant;
        private PluginConfiguration _pluginConfiguration;
        private readonly TaskExcuteConfiguration _taskExcuteConfiguration;
        private readonly IFileService _fileService;
        private readonly IPageService _pageService;
        private readonly IAssemblyService _assemblyService;



        public CollectionTaskService(
            IFileService fileService,
            IPageService pageService,
            IAssemblyService assemblyService,
            TaskExcuteConfiguration taskExcuteConfiguration,
            PluginConfiguration pluginConfiguration,
            HistoryManager<string> historyManager)
        {
            this._pluginConfiguration = pluginConfiguration;
            this._fileService = fileService;
            //this.pluginConfiguration = assemblyService.GetPluginConfigurationByName(symbol);
            //pluginProcessAssistant = assemblyService.GetProcessAssistant(pluginConfiguration);
            this._pageService = pageService;
            this._assemblyService = assemblyService;
            this._taskExcuteConfiguration = taskExcuteConfiguration;
            _historyManager = historyManager;

            _canExcute = true;

            _taskList = new Dictionary<string, object>();
            _excutingList = new Dictionary<CollectionTaskExcuteService, CancellationTokenSource>();

            LoadHistory();
            LoadTaskList();
        }


        private void LoadHistory()
        {
            var hf = Path.Combine(_pluginConfiguration.WorkSpaceRoot, ".Project", "history.his");
            if (File.Exists(hf))
                _historyManager.LoadFromFile(hf);
        }
        private void SaveHistory()
        {
            var hf = Path.Combine(_pluginConfiguration.WorkSpaceRoot, ".Project", "history.his");
            var hfFolder = Path.GetDirectoryName(hf);
            if (!Directory.Exists(hfFolder))
            {
                Directory.CreateDirectory(hfFolder);
            }
            _historyManager.SaveToFile(hf);
        }


        public Task Add(string taskUrl, object data, bool insert)
        {
            lock (_locker)
            {
                if (_taskList.ContainsKey(taskUrl))
                {
                    Logger.DefaultLogger.LogDebug("任务{0}已经在队列中", taskUrl);
                }
                else
                {
                    if (insert)
                        _taskList.Add(taskUrl, data);
                    else
                        _taskList.Add(taskUrl, data);
                    Logger.DefaultLogger.LogDebug("任务{0}已添加", taskUrl);
                    _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, $"添加新任务->{taskUrl}");
                    _pageService.TaskCountStatusChanged(_taskExcuteConfiguration, _taskList.Count);
                }
            }
            return Task.CompletedTask;
        }
        public Task Remove(string taskUrl)
        {
            lock (_locker)
            {
                if (_taskList.ContainsKey(taskUrl))
                {
                    _taskList.Remove(taskUrl);
                    _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, $"已移除任务->{taskUrl}");
                    _pageService.TaskCountStatusChanged(_taskExcuteConfiguration, _taskList.Count);
                }
            }
            return Task.CompletedTask;
        }
        public Task<string> GetCurrentTaskInfo()
        {
            return Task.FromResult(_currentTask.GetTaskInfo());
        }

        public Task<int> GetTaskCount()
        {
            return Task.FromResult(_taskList.Count);
        }
        public Task<KeyValuePair<string, object>> GetTask()
        {
            lock (_locker)
            {
                var target = _taskList.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(target.Key))
                {
                    _taskList.Remove(target.Key);
                    return Task.FromResult(target);
                }
            }
            return Task.FromResult(default(KeyValuePair<string, object>));
        }


        public async Task Excute(bool takeOne)
        {
            var browserPage = _pageService.GeneratePage();
            var task = await GetTask();
            //var pluginConfiguration = assemblyService.GetPluginConfiguration(pluginConfiguration.SymbolName);
            var processAssistant = _assemblyService.GetProcessAssistant(_pluginConfiguration);
            var cancellationSource = new CancellationTokenSource();
            _currentTask = new CollectionTaskExcuteService(
                task.Key,
                task.Value,
                browserPage.Service,
                _pageService,
                processAssistant,
                _historyManager,
                _taskExcuteConfiguration,
                cancellationSource.Token);
            _currentTask.OnNewTaskGenerated += Excuter_OnNewTaskGenerated;

            _excutingList.Add(_currentTask, cancellationSource);

            _pageService.TaskExcutStatusChanged(_taskExcuteConfiguration, true, false);
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, $"解析->{task.Key}");

            Logger.DefaultLogger.LogDebug("开始处理[{0}]", task.Key);
            await _currentTask.ExcuteTask();

            _excutingList.Remove(_currentTask);
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, $"解析完毕->{task.Key}");
            _pageService.DestroyPage(browserPage);

            if (_canExcute && !takeOne && _excutingList.Count < _maxExcute)
            {
                Task.Run(async () =>
                {
                    await Excute(takeOne);
                });
            }
            //return Task.CompletedTask;
        }

        private void Excuter_OnNewTaskGenerated(object sender, (string url, object parameter, bool fontInsert) e)
        {
            Add(e.url, e.parameter, e.fontInsert);
        }

        private void SaveTaskList()
        {
            try
            {
                var savePath = Path.Combine(_pluginConfiguration.WorkSpaceRoot, "TaskList.tl");
                _fileService.SaveToFile(_taskList, savePath);
            }
            catch (Exception e)
            {
                Logger.DefaultLogger.LogError(e, "保存任务列表失败");
            }
        }
        private void LoadTaskList()
        {
            try
            {
                var savePath = Path.Combine(_pluginConfiguration.WorkSpaceRoot, "");
                _taskList = _fileService.LoadFromFile<Dictionary<string, object>>(savePath);
            }
            catch (Exception e)
            {
                Logger.DefaultLogger.LogError(e, "载入任务列表失败");
            }
        }

        public Task Pause()
        {

            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "暂停中...");
            _excutingList.All(i => { i.Key.CanContinue = false; return true; });
            _pageService.TaskExcutStatusChanged(_taskExcuteConfiguration, true, true);
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "已暂停");
            return Task.CompletedTask;
        }

        public Task Resume()
        {
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "继续中...");
            _excutingList.All(i => { i.Key.CanContinue = true; return true; });
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "已继续");
            _pageService.TaskExcutStatusChanged(_taskExcuteConfiguration, true, false);
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _canExcute = false;

            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "停止中...");
            Task.WaitAll(_excutingList.Select(i =>
            {
                i.Value.Cancel();
                return i.Key.WaitTask();
            }).ToArray());

            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "已停止");
            _pageService.TaskExcutStatusChanged(_taskExcuteConfiguration, false, false);
            return Task.CompletedTask;
        }

        public Task Exit()
        {
            _canExcute = false;
            _cancellation.Cancel();
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "退出中...");

            Task.WaitAll(_excutingList.Select(i =>
            {
                i.Value.Cancel();
                return i.Key.WaitTask();
            }).ToArray());

            SaveHistory();
            SaveTaskList();

            return Task.CompletedTask;
        }

        public Task Update(bool updateMode)
        {
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, $"切换更新模式中...");
            _excutingList.All(i =>
            {
                i.Key.SwitchUpdateMode("", updateMode);
                return true;
            });
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, $"已应用更新模式");
            return Task.CompletedTask;
        }

        public Task Rebuild(PluginConfiguration pluginConfiguration, HistoryManager<string> historyManager)
        {
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "重建中...");
            var pluginProcessAssistant = _assemblyService.GetProcessAssistant(pluginConfiguration);
            var result = pluginProcessAssistant.RebuildHistory("", historyManager);
            result.All(i => { Add(i, null, false); return true; });
            SaveHistory();
            SaveTaskList();
            _pageService.TaskInfomationStatusChanged(_taskExcuteConfiguration, "重建完毕");
            return Task.CompletedTask;
        }

    }
}
