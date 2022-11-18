using ProcessAssistant.Configuration;
using ProcessAssistant.History;
using ProcessAssistant.Logger;
using ProcessAssistant.TaskModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant
{
    public interface IProcessAssistant
    {
        string[] GetAjaxUrlFilters();
        string[] GetDataUrlFilters();

        void OnTaskStart(string taskUrl, Dictionary<string, object> cacheStorage, object parameters,HistoryManager<string> historyManager,ILogger logger);
        void OnTaskFinished();
        void OnTaskCancelled();

        Task OnAjaxDataProcess(string symbol, string sourceUrl, MemoryStream ajaxDataStream);
        void PreviewDownloadFile(string symbol, string data, ResultStorageDescriptor storagetDescriptor);
        void OnDownloadFile(string symbol, string data, ResultStorageDescriptor storagetDescriptor);

        void PreviewSaveData(string symbol, string sourceUrl, string data, ResultStorageDescriptor storagetDescriptor);
        void OnSaveData(string symbol, string data, ResultStorageDescriptor storagetDescriptor);
        void OnSaveCachedData(string symbol, ResultStorageDescriptor storagetDescriptor);

        void SwitchUpdateMode(string symbol,bool updateMode);
        /// <summary>
        /// 对已存在的任务进行重构历史记录的操作
        /// </summary>
        /// <param name="symbol"></param>
        IEnumerable<string> RebuildHistory(string symbol, HistoryManager<string> historyManager);
        
        /// <summary>
        /// 某个数据被缓存到本地内存时被调用，用于数据校验、处理等
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="data"></param>
        void OnDataCached(string symbol, string data);

        /// <summary>
        /// 返回已产生的新任务Urls
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> OnNewTaskCreated(out object parameters);

        bool CanProcessAjaxData(string url,out string symbol);
        bool CanLoopBreak(string symbol);
    }
}
