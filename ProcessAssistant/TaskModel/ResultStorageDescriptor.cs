using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  ProcessAssistant.TaskModel
{
    public class ResultStorageDescriptor
    {
        /// <summary>
        /// 存储名
        /// </summary>
        public string Name { get; set; } 
        /// <summary>
        /// 文件后缀名
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 子路径
        /// </summary>
        public string SubPath { get; set; }

    }
}
