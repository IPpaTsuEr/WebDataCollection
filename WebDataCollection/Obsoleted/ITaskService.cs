using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Obsoleted
{
    public interface ITaskService
    {
        event EventHandler<(string url, object parameter, bool fontInsert)> OnNewTaskGenerated;
        event EventHandler<(string url, ITaskService taskService, bool isCancelled)> OnTaskFinished;
        Task ExcuteTask(string url, object Parameters);
        void CheckUpdate(string folder, string symbol);
        Task WaitTaskCancellOrFinish();
    }
}
