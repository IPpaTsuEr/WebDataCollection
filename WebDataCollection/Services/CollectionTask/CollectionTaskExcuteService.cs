using CefSharp;
using ProcessAssistant;
using ProcessAssistant.History;
using ProcessAssistant.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataCollection.Configuration;
using WebDataCollection.Interface;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Services.CollectionTask
{
    public class CollectionTaskExcuteService : WebDataCollection.Interface.CollctionTask.ITaskExcuter
    {
        private readonly string _taskUrl;
        private readonly object _parameters;
        private readonly ICEFService _cEFService;
        private readonly IPageService _pageService;
        private readonly CancellationToken _cancellationToken;
        private readonly IProcessAssistant _processAssistant;
        public TaskCompletionSource<bool> _taskCompletion;
        private readonly HistoryManager<string> _historyManager;
        private readonly TaskExcuteConfiguration _collectionTaskConfiguration;
        private Dictionary<string, object> _localParamatersStorage { get; set; } = new Dictionary<string, object>();



        public event EventHandler<(string url, object parameter, bool fontInsert)> OnNewTaskGenerated;
        public bool CanContinue { get; set; } = true;
        public bool StopRun { get; set; } = false;

        public CollectionTaskExcuteService(
            string taskUrl,
            object parameters,
            ICEFService cEFService,
            IPageService pageService,
            IProcessAssistant processAssistant,
            HistoryManager<string> historyManager,
            TaskExcuteConfiguration collectionTaskConfiguration,
            CancellationToken cancellationToken)
        {
            _taskCompletion = new TaskCompletionSource<bool>();
            this._taskUrl = taskUrl;
            this._parameters = parameters;
            this._collectionTaskConfiguration = collectionTaskConfiguration;
            this._cancellationToken = cancellationToken;
            this._processAssistant = processAssistant;
            this._historyManager = historyManager;
            this._cEFService = cEFService;
            this._pageService = pageService;
        }

        public string GetTaskInfo()
        {
            if (_localParamatersStorage.TryGetValue("TaskInfo", out var taskInfo))
            {
                return taskInfo.ToString();
            }
            return "";
        }
        public Task<bool> WaitTask()
        {
            return _taskCompletion.Task;
        }
        public Task SwitchUpdateMode(string symbol, bool updatemode)
        {
            _processAssistant.SwitchUpdateMode(symbol, updatemode);
            return Task.CompletedTask;
        }

        public async Task ExcuteTask()
        {
            try
            {
                if (_collectionTaskConfiguration != null && _processAssistant != null)
                {
                    _processAssistant.OnTaskStart(
                        _taskUrl,
                        _localParamatersStorage,
                        _parameters,
                        _historyManager,
                        Logger.DefaultLogger);

                    _cEFService.SetResourceHandlerFactory(_processAssistant);
                    _cEFService.LoadUrl(_taskUrl);

                    await _cEFService.WaitePageLoading(_cancellationToken);
                    await TaskProcesser(_collectionTaskConfiguration.Processers, _processAssistant, _parameters);

                    _processAssistant.OnTaskFinished();
                }
                _taskCompletion.TrySetResult(true);
                return;
            }
            catch (OperationCanceledException)
            {
                _pageService.TaskInfomationStatusChanged(_collectionTaskConfiguration, $"已取消->{_taskUrl}");

                Logger.DefaultLogger.LogDebug("任务[{0}]被取消", _taskUrl);
                _taskCompletion.TrySetResult(false);
            }
            catch (Exception e)
            {
                _pageService.TaskInfomationStatusChanged(_collectionTaskConfiguration, $"出现异常->{_taskUrl}");
                Logger.DefaultLogger.LogError(e, "任务[{0}]出现异常", _taskUrl);
                _taskCompletion.TrySetResult(false);
            }

        }

        private async Task TaskProcesser(List<TaskDescriptor> taskDescriptors, IProcessAssistant processAssistant, object Parameters)
        {
            for (int index = 0; index < taskDescriptors.Count; index++)
            {
                var i = taskDescriptors[index];
                JavascriptResponse result = null;
                var resultData = string.Empty;

                while (!CanContinue)
                {
                    await Task.Delay(300);
                    _cancellationToken.ThrowIfCancellationRequested();
                }

                _cancellationToken.ThrowIfCancellationRequested();

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
                _cancellationToken.ThrowIfCancellationRequested();

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
                        processAssistant.SwitchUpdateMode(i.Symbol, true);
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
