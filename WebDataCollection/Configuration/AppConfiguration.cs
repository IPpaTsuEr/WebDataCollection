using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Configuration
{
    public class AppConfiguration
    {
        public string TaskConfigurationStoragePath{ get; set; }
        public int MaxTaskRunCount { get; set; } = 1;
        public string HomePage { get; set; }
        public string TaskQueueSaveFileName { get; set; } = "DefaultTaskList.tsk";
        public string LogPath { get; set; } = "Logs";
        public bool AutoLaunch { get; set; }
    }
}
