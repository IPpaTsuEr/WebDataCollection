using ProcessAssistant.Configuration;
using ProcessAssistant.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Interface.CollctionTask
{
    public interface ITaskService
    {
        Task Excute(bool takeOne);
        Task Stop();
        Task Pause();
        Task Resume();

        Task Add(string taskUrl, object parameters, bool insertFirst);
        Task Remove(string taskUrl);

        Task<int> GetTaskCount();
        Task<string> GetCurrentTaskInfo();

        Task Update(bool updateMode);
        Task Rebuild(PluginConfiguration pluginConfiguration, HistoryManager<string> historyManager);
    }
    
}
