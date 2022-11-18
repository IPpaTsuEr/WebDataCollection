using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WebDataCollection.Model;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Interface
{
    public interface IPageService
    {
        BrowserPage GeneratePage();
        void DestroyPage(BrowserPage browserPage);
        void TaskStart(TaskExcuteConfiguration target);
        void TaskStop(TaskExcuteConfiguration target);
        void TaskInfomationStatusChanged(TaskExcuteConfiguration target,string information);
        void TaskCountStatusChanged(TaskExcuteConfiguration target,int taskListCount);
        void TaskExcutStatusChanged(TaskExcuteConfiguration target,bool isExcuting, bool isPaued);
    }
}
