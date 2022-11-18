using ProcessAssistant.Configuration;
using ProcessAssistant.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataCollection.Configuration;
using WebDataCollection.Interface;
using WebDataCollection.Model;
using WebDataCollection.Services;
using IFileService = ProcessAssistant.File.IFileService;

namespace WebDataCollection.Obsoleted
{
    public class TaskManager : ITaskManager
    {
        private readonly IFileService _fileService;
        private readonly IAppConfigurationService _configuration;
        private readonly IAssemblyService _assemblyService;


        private Semaphore _excuteSemaphore;
        private ConcurrentDictionary<string, object> _taskQueue;
        private ConcurrentDictionary<BrowserPage, CancellationTokenSource> _cancellQueue;
        private List<CancellationTokenSource> _cancelList;
        private bool _canExcuteTask = true;
        private string _saveFilePath;
        private ConcurrentDictionary<ITaskService, int> _ExcutingQueue = new ConcurrentDictionary<ITaskService, int>();

        public event EventHandler<(string url, BrowserPage page)> OnTaskComplected;
        public event EventHandler<BrowserPage> OnTaskStarted;

        public bool TaskNotRuned { get; set; }
        public TaskManager(IFileService fileService, IAppConfigurationService configuration, IAssemblyService assemblyService)
        {
            _fileService = fileService;
            _configuration = configuration;
            _assemblyService = assemblyService;

            _excuteSemaphore = new Semaphore(_configuration.ApplacationConfiguration.MaxTaskRunCount, _configuration.ApplacationConfiguration.MaxTaskRunCount);
            _cancelList = new List<CancellationTokenSource>();
            _taskQueue = new ConcurrentDictionary<string, object>();
            _cancellQueue = new ConcurrentDictionary<BrowserPage, CancellationTokenSource>();
            TaskNotRuned = true;

            var saveFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            _saveFilePath = Path.Combine(saveFolder, _configuration.ApplacationConfiguration.TaskQueueSaveFileName);
        }

        public void CreatTask(string url, object parameter, bool fontInsert = false)
        {
            try
            {
                if (_taskQueue.ContainsKey(url))
                {
                    Logger.DefaultLogger.LogDebug("任务已在队列中：{0}", url);
                }
                else
                {
                    _taskQueue.TryAdd(url, parameter);
                    Logger.DefaultLogger.LogDebug("任务已添加到队列：{0}", url);
                }
            }
            catch (Exception e)
            {
                Logger.DefaultLogger.LogError(e, "执行添加任务操作失败");
            }
        }

        public async Task RunTask(BrowserPage browserPage)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            ITaskService taskService = new TaskService(browserPage, _assemblyService, _configuration, cancellation.Token);
            taskService.OnNewTaskGenerated += TaskService_OnNewTaskGenerated;
            _cancellQueue.TryAdd(browserPage, cancellation);

            await taskService.ExcuteTask(browserPage.Url, null);
        }

        public void CancellTask(BrowserPage browserPage)
        {
            if (_cancellQueue.TryGetValue(browserPage, out var cancellation))
            {
                cancellation.Cancel();
            }
        }

        public void CheckUpDate(string url)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            ITaskService taskService = new TaskService(null, _assemblyService, _configuration, cancellation.Token);
            taskService.OnNewTaskGenerated += TaskService_OnNewTaskGenerated;
            taskService.CheckUpdate("", "");
        }

        public Task<CancellationTokenSource> ExcuteTask()
        {
            if (!_canExcuteTask)
                return null;

            TaskNotRuned = false;

            Logger.DefaultLogger.LogDebug("执行任务：等待信号");
            if (_excuteSemaphore.WaitOne())
            {
                Logger.DefaultLogger.LogDebug("执行任务：取得信号");
                var kv = _taskQueue.FirstOrDefault();
                try
                {

                    Logger.DefaultLogger.LogDebug("执行任务：取得任务{0}", kv.Key);
                    _taskQueue.TryRemove(kv.Key, out _);
                }
                catch { }


                if (!string.IsNullOrWhiteSpace(kv.Key))
                {
                    BrowserPage browserPage = null;
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        browserPage = new BrowserPage();
                        OnTaskStarted(this, browserPage);
                    });

                    CancellationTokenSource cancellation = new CancellationTokenSource();
                    ITaskService taskService = new TaskService(browserPage, _assemblyService, _configuration, cancellation.Token);
                    taskService.OnNewTaskGenerated += TaskService_OnNewTaskGenerated;
                    taskService.OnTaskFinished += TaskService_OnTaskFinished;
                    _cancelList.Add(cancellation);
                    _ExcutingQueue.TryAdd(taskService, 0);
                    Task.Run(async () =>
                    {
                        await taskService.ExcuteTask(kv.Key, kv.Value);

                        _cancelList.Remove(cancellation);
                        Logger.DefaultLogger.LogDebug("执行任务：准备释放信号");
                        _excuteSemaphore.Release(1);
                        Logger.DefaultLogger.LogDebug("执行任务：已释放信号");

                        OnTaskComplected?.Invoke(this, (kv.Key, browserPage));

                        if (_configuration.ApplacationConfiguration.AutoLaunch)
                        {
                            if (_taskQueue.Count == 0)
                            {
                                Logger.DefaultLogger.LogDebug("执行任务：任务队列为空");
                            }
                            else
                            {
                                await ExcuteTask();
                            }
                        }
                    });

                    return Task.FromResult(cancellation);
                }
                else
                {
                    _excuteSemaphore.Release(1);
                }
                return null;
            }
            return null;
        }

        private void TaskService_OnTaskFinished(object sender, (string url, ITaskService taskService, bool isCancelled) e)
        {
            if (_canExcuteTask)
                _ExcutingQueue.TryRemove(e.taskService, out _);
        }

        private void TaskService_OnNewTaskGenerated(object sender, (string url, object parameter, bool fontInsert) e)
        {
            CreatTask(e.url, e.parameter, e.fontInsert);
        }

        public void CancelAllTask()
        {
            _canExcuteTask = false;
            _cancelList.ForEach(i => i.Cancel());
            _cancelList.Clear();
            Task.WaitAll(_ExcutingQueue.Select(i => i.Key.WaitTaskCancellOrFinish()).ToArray());
            _ExcutingQueue.Clear();
        }

        public int GetExcutingTaskCount()
        {
            return _cancelList.Count;
        }

        public int GetWattingTaskCount()
        {
            return _taskQueue.Count;
        }

        /// <summary>
        /// 取消当前所有任务然后退出
        /// </summary>
        public void Exite()
        {
            //if (TaskNotRuned)
            //    return;
            _canExcuteTask = false;
            CancelAllTask();
            _fileService.SaveToFile(_taskQueue, _saveFilePath);
            TaskNotRuned = true;
        }
        /// <summary>
        /// 等待当前任务执行完毕后退出,并保存当前未完成的任务
        /// </summary>
        public async Task WaitAndExite()
        {
            //if (TaskNotRuned)
            //    return;
            _canExcuteTask = false;
            while (GetExcutingTaskCount() == 0)
            {
                await Task.Delay(1000);
            }
            _fileService.SaveToFile(_taskQueue, _saveFilePath);
            TaskNotRuned = true;
        }
    }
}
