using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json.Linq;
using ProcessAssistant;
using ProcessAssistant.History;
using ProcessAssistant.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataCollection.Configuration;
using WebDataCollection.Interface;
using WebDataCollection.Model;
using WebDataCollection.Model.CollectionTask;
using WebDataCollection.Services;
using IFileService = ProcessAssistant.File.IFileService;

namespace WebDataCollection.Obsoleted
{
    public class TaskService : ITaskService
    {
        private BrowserPage _browserPage { get; set; }
        private IAssemblyService _assemblyService { get; set; }
        private ICEFService _cEFService { get; set; }
        private IAppConfigurationService _configurationService { get; }

        private CancellationToken _cancellation { get; }

        public event EventHandler<(string url, object parameter, bool fontInsert)> OnNewTaskGenerated;
        public event EventHandler<(string url, ITaskService taskService, bool isCancelled)> OnTaskFinished;

        private TaskCompletionSource<bool> TaskFinishedSource = new TaskCompletionSource<bool>();

        private HistoryManager<string> historyManager;
        public TaskService(
            BrowserPage browserPage,
            IAssemblyService assemblyService,
            IAppConfigurationService configurationService,
            CancellationToken cancellation
            )
        {
            _browserPage = browserPage;
            _assemblyService = assemblyService;
            _configurationService = configurationService;
            _cancellation = cancellation;
            _cEFService = _browserPage.Service;
            historyManager = new HistoryManager<string>();
        }

        private Dictionary<string, object> _localParamatersStorage { get; set; } = new Dictionary<string, object>();

        public async Task WaitTaskCancellOrFinish()
        {
            await TaskFinishedSource.Task;
        }
        public void CheckUpdate(string url, string symbol)
        {
            var taskProccess = _assemblyService.GetProcessAssistant(url);
            taskProccess.SwitchUpdateMode(symbol, true);
        }

        public async Task ExcuteTask(string url, object Parameters)
        {
            var taskConfig = _configurationService.GetTaskConfiguration(url);
            var taskProccess = _assemblyService.GetProcessAssistant(url);

            if (taskConfig != null && taskProccess != null)
            {
                try
                {
                    taskProccess.OnTaskStart(
                        url, _localParamatersStorage,
                        Parameters,
                        historyManager,
                        Logger.DefaultLogger);

                    _browserPage.Service.SetResourceHandlerFactory(taskProccess);
                    _browserPage.Service.LoadUrl(url);

                    await WaitePageLoading();
                    await TaskProcesser(taskConfig.Processers, taskProccess, Parameters);
                    taskProccess.OnTaskFinished();

                    TaskFinishedSource.TrySetResult(false);
                    OnTaskFinished?.Invoke(this, (url, this, false));
                }
                catch (OperationCanceledException)
                {
                    taskProccess.OnTaskCancelled();

                    TaskFinishedSource.TrySetResult(true);
                    OnTaskFinished?.Invoke(this, (url, this, true));
                }
                catch (Exception error)
                {
                    Logger.DefaultLogger.LogError(error, "执行任务相关操作失败");


                    TaskFinishedSource.TrySetResult(false);
                    OnTaskFinished?.Invoke(this, (url, this, false));
                }
            }
            else
            {
                Logger.DefaultLogger.LogDebug("执行任务相关对象为空");

                TaskFinishedSource.TrySetResult(false);
                OnTaskFinished?.Invoke(this, (url, this, false));
            }

        }


        private async Task WaitePageLoading()
        {
            while (true)
            {
                await Task.Delay(500);
                _cancellation.ThrowIfCancellationRequested();
                var isLodede = System.Windows.Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        return _browserPage.Service.GetBrowser().GetBrowser().IsLoading;
                    });
                if (!isLodede)
                    break;
            }
        }

        private async Task TaskProcesser(List<TaskDescriptor> taskDescriptors, IProcessAssistant processAssistant, object Parameters)
        {
            for (int index = 0; index < taskDescriptors.Count; index++)
            {
                var i = taskDescriptors[index];
                JavascriptResponse result = null;
                var resultData = string.Empty;

                _cancellation.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(i.Code))
                {
                    result = await _cEFService.ExcuteScript(i.Code);
                    resultData = result.Result as string;
                    if (result.Success == false)
                        throw new Exception($"执行脚本{i.Code}失败：{result.Message}");
                    else
                        Logger.DefaultLogger.LogDebug("执行脚本{0}成功：{1}", i.ResultType, i.Code);
                }
                else
                {

                }
                _cancellation.ThrowIfCancellationRequested();

                switch (i.ResultType)
                {
                    case TaskResultType.DownloadFile:
                        processAssistant.OnDownloadFile(i.Symbol, resultData, i.StorageDescriptor);
                        break;
                    case TaskResultType.SaveData:
                        processAssistant.OnSaveData(i.Symbol, resultData, i.StorageDescriptor);
                        break;
                    case TaskResultType.TaskUrl:
                        OnNewTaskGenerated?.Invoke(this, (resultData, Parameters, false));
                        break;
                    case TaskResultType.CacheData:
                        _localParamatersStorage.Add(i.Symbol, resultData);
                        processAssistant.OnDataCached(i.Symbol, resultData);
                        break;
                    case TaskResultType.SaveCacheData:
                        processAssistant.OnSaveCachedData(i.Symbol, i.StorageDescriptor);
                        break;
                    case TaskResultType.None:
                        break;
                    case TaskResultType.Loop:
                        Logger.DefaultLogger?.LogDebug("开始循环条件检查：{0}", i.Symbol);
                        while (!processAssistant.CanLoopBreak(i.Symbol))
                        {
                            await TaskProcesser(i.Processers, processAssistant, Parameters);
                        }
                        break;
                    case TaskResultType.Delay:
                        await Task.Delay(int.Parse(i.Symbol));
                        break;
                    case TaskResultType.CheckUpdate:
                        // processAssistant.CheckUpdate(i.Symbol);
                        break;
                    default:
                        break;
                }
                processAssistant.OnNewTaskCreated(out var newTaskParameters)
                        ?.ToList()
                        .ForEach(nl =>
                        {
                            OnNewTaskGenerated?.Invoke(this, (nl, newTaskParameters, false));
                        }
                        );
            }
        }


    }
}
