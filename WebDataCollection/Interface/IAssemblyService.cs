using ProcessAssistant;
using ProcessAssistant.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Interface
{
    public interface IAssemblyService
    {
        PluginConfiguration GetPluginConfiguration(string hostName);
        PluginConfiguration GetPluginConfigurationByName(string pluginDisplayName);
        List<PluginConfiguration> GetFoundedPlugins();
        IProcessAssistant GetProcessAssistant(string url);
        IProcessAssistant GetProcessAssistant(PluginConfiguration pluginConfiguration);
    }
}
