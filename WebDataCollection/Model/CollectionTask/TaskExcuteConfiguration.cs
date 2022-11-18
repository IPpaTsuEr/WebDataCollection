using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Model.CollectionTask
{
    //任务基本信息
    public class TaskExcuteConfiguration
    {
        public string Name { get; set; }
        public string MatchRegex { get; set; }
        public List<TaskDescriptor> Processers { get; set; }
    }
}
