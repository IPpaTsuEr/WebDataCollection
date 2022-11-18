using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataCollection.Model;

namespace WebDataCollection.Obsoleted
{
    public interface ITaskManager
    {
        event EventHandler<(string url, BrowserPage page)> OnTaskComplected;
        event EventHandler<BrowserPage> OnTaskStarted;

        void CreatTask(string url, object parameters, bool fontInsert = false);
        Task<CancellationTokenSource> ExcuteTask();
        void CancelAllTask();
        int GetExcutingTaskCount();
        int GetWattingTaskCount();

        /// <summary>
        /// 取消当前所有任务然后退出
        /// </summary>
        void Exite();
        /// <summary>
        /// 等待当前任务执行完毕后退出,并保存当前未完成的任务
        /// </summary>
        Task WaitAndExite();
    }
}
