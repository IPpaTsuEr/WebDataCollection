using CefSharp.Wpf;
using ProcessAssistant.File;
using ProcessAssistant.Logger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WebDataCollection.Command;
using WebDataCollection.Configuration;
using WebDataCollection.Interface;
using WebDataCollection.Model;
using WebDataCollection.Model.CollectionTask;
using WebDataCollection.Notify;
using WebDataCollection.Services;
using WebDataCollection.Services.CollectionTask;

namespace WebDataCollection.ViewModel
{
    public class MainViewModel : MvvmNotify, IPageService
    {
        #region Properties

        private BrowserPage _SelectedBrowser;
        public BrowserPage SelectedBrowser
        {
            get { return _SelectedBrowser; }
            set { _SelectedBrowser = value; Notify(); }
        }

        //private string _CurrentUrl;
        //public string CurrentUrl
        //{
        //    get { return _CurrentUrl; }
        //    set { _CurrentUrl = value; Notify(); }
        //}

        private ObservableCollection<BrowserPage> _Browsers;
        public ObservableCollection<BrowserPage> Browsers
        {
            get { return _Browsers; }
            set { _Browsers = value; Notify(); }
        }

        private ObservableCollection<CollectionPlugin> _FoundedPlugins;

        public ObservableCollection<CollectionPlugin> FoundedPlugins
        {
            get { return _FoundedPlugins; }
            set { _FoundedPlugins = value; Notify(); }
        }

        #endregion

        #region Commands

        private MvvmCommand _F12Command;
        public MvvmCommand F12Command
        {
            get
            {
                return _F12Command;
            }
            set
            {
                _F12Command = value;
                Notify();
            }
        }

        private MvvmCommand _F5Command;
        public MvvmCommand F5Command
        {
            get
            {
                return _F5Command;
            }
            set
            {
                _F5Command = value;
                Notify();
            }
        }


        private MvvmCommand _JumpToCommand;
        public MvvmCommand JumpToCommand
        {
            get
            {
                return _JumpToCommand;
            }
            set
            {
                _JumpToCommand = value;
                Notify();
            }
        }


        private MvvmCommand<BrowserPage> _ClosePageCommand;
        public MvvmCommand<BrowserPage> ClosePageCommand
        {
            get
            {
                return _ClosePageCommand;
            }
            set
            {
                _ClosePageCommand = value;
            }
        }

        private MvvmCommand _ExitCommand;
        public MvvmCommand ExitCommand
        {
            get
            {
                return _ExitCommand;
            }
            set
            {
                _ExitCommand = value;
            }
        }

        private MvvmCommand<CollectionPlugin> _StartTaskCommand;
        public MvvmCommand<CollectionPlugin> StartTaskCommand
        {
            get
            {
                return _StartTaskCommand;
            }
            set
            {
                _StartTaskCommand = value;
                Notify();
            }
        }

        private MvvmCommand<CollectionPlugin> _StopTaskCommand;
        public MvvmCommand<CollectionPlugin> StopTaskCommand
        {
            get
            {
                return _StopTaskCommand;
            }
            set
            {
                _StopTaskCommand = value;
                Notify();
            }
        }
        private MvvmCommand<CollectionPlugin> _PauseTaskCommand;
        public MvvmCommand<CollectionPlugin> PauseTaskCommand
        {
            get
            {
                return _PauseTaskCommand;
            }
            set
            {
                _PauseTaskCommand = value;
                Notify();
            }
        }
        private MvvmCommand<CollectionPlugin> _ResumeTaskCommand;
        public MvvmCommand<CollectionPlugin> ResumeTaskCommand
        {
            get
            {
                return _ResumeTaskCommand;
            }
            set
            {
                _ResumeTaskCommand = value;
                Notify();
            }
        }

        private MvvmCommand<CollectionPlugin> _RebuildCommand;
        public MvvmCommand<CollectionPlugin> RebuildCommand
        {
            get
            {
                return _RebuildCommand;
            }
            set
            {
                _RebuildCommand = value;
            }
        }

        private MvvmCommand _HomePageCommand;
        public MvvmCommand HomePageCommand
        {
            get { return _HomePageCommand; }
            set { _HomePageCommand = value; }
        }

        private MvvmCommand _AddTaskCommand;
        public MvvmCommand AddTaskCommand
        {
            get { return _AddTaskCommand; }
            set { _AddTaskCommand = value; }
        }

        private MvvmCommand _NewPageCommand;
        public MvvmCommand NewPageCommand
        {
            get { return _NewPageCommand; }
            set { _NewPageCommand = value; }
        }


        #endregion

        #region Services

        IFileService fileService;
        //ITaskManager taskManager { get; set; }
        IAppConfigurationService configuration;
        IAssemblyService assemblyService;
        ILogger logger;

        CollectionTaskManager collectionTaskManager { get; set; }

        #endregion

        public MainViewModel()
        {
            assemblyService = new AssemblyService();
            fileService = new FileService();
            configuration = new AppConfigurationService(fileService);

            collectionTaskManager = new CollectionTaskManager(fileService, configuration, assemblyService, this);

            //taskService = new CollectionTaskService("",this,assemblyService);

            AddTaskCommand = new MvvmCommand(OnAddTask);
            RebuildCommand = new MvvmCommand<CollectionPlugin>(OnRebuild);
            StartTaskCommand = new MvvmCommand<CollectionPlugin>(OnStartTask);
            StopTaskCommand = new MvvmCommand<CollectionPlugin>(OnStopTask);
            PauseTaskCommand = new MvvmCommand<CollectionPlugin>(OnPauseTask);
            ResumeTaskCommand = new MvvmCommand<CollectionPlugin>(OnResumeask);

            F5Command = new MvvmCommand(OnF5);
            F12Command = new MvvmCommand(OnF12);
            HomePageCommand = new MvvmCommand(OnHomePage);
            NewPageCommand = new MvvmCommand(OnNewPage);
            JumpToCommand = new MvvmCommand(OnJumpTo);
            ClosePageCommand = new MvvmCommand<BrowserPage>(OnClosePage);
            ExitCommand = new MvvmCommand(OnExit);

            Browsers = new ObservableCollection<BrowserPage>();
            FoundedPlugins = new ObservableCollection<CollectionPlugin>();
            assemblyService.GetFoundedPlugins().ForEach(n =>
            {
                var pl = new CollectionPlugin();
                pl.DisplayName = n.DisplayName;
                pl.PluginConfig = n;
                pl.ExcuteConfig = configuration.GetTaskConfigurationByName(n.DisplayName);
                FoundedPlugins.Add(pl);
            });


            var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration.ApplacationConfiguration.LogPath);
            logger = Logger.Create(logPath);
            Logger.DefaultLogger = logger;

            //default page
            var b = new BrowserPage();
            b.Service.LoadUrl("http://www.weibo.com");
            b.Service.OnNewWindowPopup += Service_OnNewWindowPopup;
            Browsers.Add(b);

        }

        #region CommandMethods

        private void OnNewPage()
        {
            var targetUrl = configuration.ApplacationConfiguration.HomePage;

            SelectedBrowser = GenerateDefaultPage(targetUrl);
        }
        private void OnF5()
        {
            SelectedBrowser.Service.Refresh();
        }

        private void OnHomePage()
        {
            var targetUrl = configuration.ApplacationConfiguration.HomePage;
            if (SelectedBrowser == null)
            {
                SelectedBrowser = GenerateDefaultPage(targetUrl);
            }
            else
            {
                SelectedBrowser.Service.LoadUrl(configuration.ApplacationConfiguration.HomePage);
            }

        }

        private void OnExit()
        {
            collectionTaskManager.StopAll().Wait();
        }

        private BrowserPage GenerateDefaultPage(string url = "https://www.baidu.com")
        {
            var b = new BrowserPage();
            b.Service.LoadUrl(url);
            b.Service.OnNewWindowPopup += Service_OnNewWindowPopup;
            Browsers.Add(b);
            return b;
        }
        private void OnClosePage(BrowserPage browser)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (browser != null)
                {
                    browser.Service.GetBrowser().GetBrowser().CloseBrowser(false);
                    Browsers.Remove(browser);
                }
                if (Browsers.Count == 0)
                {
                    SelectedBrowser = GenerateDefaultPage();
                }
            });
        }

        private void TaskManager_OnTaskComplected(object sender, (string url, BrowserPage page) e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Browsers.Remove(e.page);
                if (SelectedBrowser == e.page)
                {
                    SelectedBrowser = Browsers.First();
                }
            });
        }

        private void TaskManager_OnTaskStarted(object sender, BrowserPage e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Browsers.Add(e);
                SelectedBrowser = e;
            });
        }

        private void OnJumpTo()
        {
            var newUrl = string.Empty;
            if (SelectedBrowser != null)
            {
                newUrl = SelectedBrowser.Url;
            }
            else
            {
                newUrl = "https://www.baidu.com";
            }
            SelectedBrowser = GenerateDefaultPage(newUrl);
        }


        private void Service_OnNewWindowPopup(object sender, string e)
        {
            App.Current.Dispatcher.Invoke(() =>
             {
                 var b = new BrowserPage();
                 b.Service.LoadUrl(e);
                 b.Service.OnNewWindowPopup += Service_OnNewWindowPopup;

                 Browsers.Add(b);
                 SelectedBrowser = b;
             });
        }

        private void OnF12()
        {
            SelectedBrowser.Service.DevToolsSwitcher();
        }

        public BrowserPage GeneratePage()
        {
            BrowserPage page = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                page = new BrowserPage();
                Browsers.Add(page);
                SelectedBrowser = page;
            });
            return page;
        }

        public void DestroyPage(BrowserPage browserPage)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Browsers.Remove(browserPage);
                if (SelectedBrowser == browserPage)
                {
                    SelectedBrowser = Browsers.First();
                }
            });
        }


        private void OnRebuild(CollectionPlugin obj)
        {
            //var plugincf = assemblyService.GetPluginConfigurationByName(obj);
            //var instance = assemblyService.GetProcessAssistant(plugincf);
            //var tasks = instance.RebuildHistory("",new ProcessAssistant.History.HistoryManager<string>());
            //tasks.All(i => { taskManager.CreatTask(i,null);return true; });

            collectionTaskManager.RebuildCollectionTask(obj.DisplayName);
        }

        public void TaskStart(TaskExcuteConfiguration target)
        {
            var p = FoundedPlugins.FirstOrDefault(p => p.ExcuteConfig == target);
            if (p != null)
            {
                p.IsExcuting = true;

            }
        }

        private void OnAddTask()
        {
            if (SelectedBrowser != null)
            {
                var taskUrl = SelectedBrowser.Url;
                collectionTaskManager.StartCollectionTask(taskUrl);
            }
        }

        public void TaskStop(TaskExcuteConfiguration target)
        {
            var p = FoundedPlugins.FirstOrDefault(p => p.ExcuteConfig == target);
            if (p != null)
            {
                p.IsExcuting = false;
            }
        }

        private void OnResumeask(CollectionPlugin obj)
        {
            collectionTaskManager.ResumeCollectionTask(obj.ExcuteConfig);
        }

        private void OnPauseTask(CollectionPlugin obj)
        {
            collectionTaskManager.PauseCollectionTask(obj.ExcuteConfig);
        }


        private void OnStopTask(CollectionPlugin obj)
        {
            Task.Run(async () =>
            {
                await  collectionTaskManager.StopCollectionTask(obj.ExcuteConfig);
            });
        }

        private void OnStartTask(CollectionPlugin obj)
        {
            Task.Run(async () =>
            {
                await collectionTaskManager.StartCollectionTask(obj.ExcuteConfig);
            });
        }

        public void TaskInfomationStatusChanged(TaskExcuteConfiguration target, string information)
        {
            var p = FoundedPlugins.FirstOrDefault(p => p.ExcuteConfig == target);
            if (p != null)
            {
                p.CurrentInfo = information;
            }
        }

        public void TaskCountStatusChanged(TaskExcuteConfiguration target, int taskListCount)
        {
            var p = FoundedPlugins.FirstOrDefault(p => p.ExcuteConfig == target);
            if (p != null)
            {
                p.TaskListCount = taskListCount;
            }
        }

        public void TaskExcutStatusChanged(TaskExcuteConfiguration target, bool isExcuting, bool isPaued)
        {
            var p = FoundedPlugins.FirstOrDefault(p => p.ExcuteConfig == target);
            if (p != null)
            {
                p.IsExcuting = isExcuting;
                p.IsPaused = isPaued;
            }
        } 

        #endregion


    }
}
