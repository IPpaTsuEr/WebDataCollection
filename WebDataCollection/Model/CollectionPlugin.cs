using ProcessAssistant.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Model.CollectionTask;
using WebDataCollection.Notify;

namespace WebDataCollection.Model
{
    public class CollectionPlugin : MvvmNotify
    {
        private string _DisplayName;

        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; Notify(); }
        }

        private string _CurrentInfo;

        public string CurrentInfo
        {
            get { return _CurrentInfo; }
            set { _CurrentInfo = value; Notify(); }
        }

        private bool _IsExcuting = false;

        public bool IsExcuting
        {
            get { return _IsExcuting; }
            set
            {
                _IsExcuting = value;
                Notify();
                if (value)
                { IsPaused = false; }
            }
        }
        private bool _IsPaused = false;

        public bool IsPaused
        {
            get { return _IsPaused; }
            set
            {
                _IsPaused = value;
                Notify();
            }
        }
        private int _TaskListCount;

        public int TaskListCount
        {
            get { return _TaskListCount; }
            set { _TaskListCount = value; Notify(); }
        }


        public TaskExcuteConfiguration ExcuteConfig { get; set; }
        public PluginConfiguration PluginConfig { get; set; }
    }
}
