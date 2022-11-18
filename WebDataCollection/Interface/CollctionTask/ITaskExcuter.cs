using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Interface.CollctionTask
{
    interface ITaskExcuter
    {
        string GetTaskInfo();
        Task<bool> WaitTask();
        Task SwitchUpdateMode(string symbol, bool updatemode);
        Task ExcuteTask();
    }
}
