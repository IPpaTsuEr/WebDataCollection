using ProcessAssistant.TaskModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Model.CollectionTask
{

    //任务操作描述
    public class TaskDescriptor
    {
        public string Symbol { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskResultType ResultType { get; set; }
        public ResultStorageDescriptor StorageDescriptor { get; set; }
        public List<TaskDescriptor> Processers { get; set; }
    }
}
