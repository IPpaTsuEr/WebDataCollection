using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Interface.CollctionTask
{
    interface ITaskManager
    {
        Task StartCollectionTask(string url);
        Task StartCollectionTask(TaskExcuteConfiguration taskExcuteCfg);
        Task StopCollectionTask(TaskExcuteConfiguration taskExcuteCfg);
        Task PauseCollectionTask(TaskExcuteConfiguration taskExcuteCfg);
        Task ResumeCollectionTask(TaskExcuteConfiguration taskExcuteCfg);
        Task UpdateCollectionTask(TaskExcuteConfiguration taskExcuteCfg, bool updateMode);
        Task StopAll();
        Task<bool> RebuildCollectionTask(string taskName);

    }
}
