using ProcessAssistant.File;
using ProcessAssistant.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Configuration;
using WebDataCollection.Interface;
using WebDataCollection.Model;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Services.CollectionTask
{
    public class CollectionTaskManager : WebDataCollection.Interface.CollctionTask.ITaskManager
    {
        private readonly IFileService _fileService;
        private readonly IAppConfigurationService _appConfigurationService;
        private readonly IAssemblyService _assemblyService;
        private readonly IPageService _pageService;

        private Dictionary<TaskExcuteConfiguration, HistoryManager<string>> _taskHistoryMap = new Dictionary<TaskExcuteConfiguration, HistoryManager<string>>();
        private Dictionary<TaskExcuteConfiguration, CollectionTaskService> _taskServiceMap = new Dictionary<TaskExcuteConfiguration, CollectionTaskService>();

        public CollectionTaskManager(
            IFileService fileService,
            IAppConfigurationService appConfigurationService,
            IAssemblyService assemblyService,
            IPageService pageService
            )
        {
            this._fileService = fileService;
            this._appConfigurationService = appConfigurationService;
            this._assemblyService = assemblyService;
            this._pageService = pageService;
        }

        private Task<CollectionTaskService> FindOrCreateTaskService(TaskExcuteConfiguration taskExcuteCfg)
        {
            if (!_taskServiceMap.TryGetValue(taskExcuteCfg, out var service))
            {
                if (!_taskHistoryMap.TryGetValue(taskExcuteCfg, out var history))
                {
                    history = new HistoryManager<string>();
                    _taskHistoryMap.Add(taskExcuteCfg, history);
                }
                var pluginConfiguration = _assemblyService.GetPluginConfigurationByName(taskExcuteCfg.Name);
                service = new CollectionTaskService(
                    _fileService,
                   _pageService,
                   _assemblyService,
                   taskExcuteCfg,
                   pluginConfiguration,
                   history
                   );
                _taskServiceMap.Add(taskExcuteCfg, service);
            }
            return Task.FromResult(service);
        }

        public async Task StartCollectionTask(string url)
        {
            var taskConfig = _appConfigurationService.GetTaskConfiguration(url);
            //if (!_taskHistoryMap.TryGetValue(taskConfig, out var history))
            //{
            //    history = new HistoryManager<string>();
            //    _taskHistoryMap.Add(taskConfig, history);
            //}
            //var pluginConfiguration = assemblyService.GetPluginConfigurationByName(taskConfig.Name);
            //CollectionTaskService taskService = new CollectionTaskService(
            //    pageService,
            //    assemblyService,
            //    taskConfig,
            //    pluginConfiguration,
            //    history
            //    );
            //_taskServiceMap.Add(taskConfig, taskService);
            //pageService.TaskStart(taskConfig);
            //await taskService.Excute(false, (s, i) =>
            //{
            //    pageService.TaskStatus(taskConfig,s, i);
            //});
            //pageService.TaskStop(taskConfig);
            var service = await FindOrCreateTaskService(taskConfig);
            await service.Add(url, null, true);
            await StartCollectionTask(taskConfig);
        }

        public async Task StartCollectionTask(TaskExcuteConfiguration taskExcuteCfg)
        {
            var service = await FindOrCreateTaskService(taskExcuteCfg);
            _pageService.TaskStart(taskExcuteCfg);
            await service.Excute(false);
            _pageService.TaskStop(taskExcuteCfg);

        }
        public async Task StopCollectionTask(TaskExcuteConfiguration taskExcuteCfg)
        {
            if (_taskServiceMap.TryGetValue(taskExcuteCfg, out var service))
            {
                await service.Stop();
            }
        }
        public async Task PauseCollectionTask(TaskExcuteConfiguration taskExcuteCfg)
        {
            if (_taskServiceMap.TryGetValue(taskExcuteCfg, out var service))
            {
                await service.Pause();
            }
        }
        public async Task ResumeCollectionTask(TaskExcuteConfiguration taskExcuteCfg)
        {
            if (_taskServiceMap.TryGetValue(taskExcuteCfg, out var service))
            {
                await service.Resume();
            }
        }
        public async Task UpdateCollectionTask(TaskExcuteConfiguration taskExcuteCfg, bool updateMode)
        {
            if (_taskServiceMap.TryGetValue(taskExcuteCfg, out var service))
            {
                await service.Update(updateMode);
            }
        }


        public Task StopAll()
        {
            _taskServiceMap.Values.ToList().ForEach(async i => await i.Exit());
            return Task.CompletedTask;
        }

        public async Task<bool> RebuildCollectionTask(string taskName)
        {
            var collectionConfiguration = _appConfigurationService.GetTaskConfiguration(taskName);
            if (collectionConfiguration is null)
                return false;
            var pluginConfig = _assemblyService.GetPluginConfigurationByName(taskName);
            if (pluginConfig is null)
                return false;
            if (!_taskHistoryMap.TryGetValue(collectionConfiguration, out var history))
            {
                history = new HistoryManager<string>();
                _taskHistoryMap.Add(collectionConfiguration, history);
            }
            if (!_taskServiceMap.TryGetValue(collectionConfiguration, out var service))
            {
                service = new CollectionTaskService(_fileService, _pageService, _assemblyService, collectionConfiguration, pluginConfig, history);
                _taskServiceMap.Add(collectionConfiguration, service);
            }
            else
            {
                await service.Rebuild(pluginConfig, history);
            }
            return true;
        }
    }
}
