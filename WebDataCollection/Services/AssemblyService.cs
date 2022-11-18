using ProcessAssistant;
using ProcessAssistant.Configuration;
using ProcessAssistant.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Interface;

namespace WebDataCollection.Services
{
    public class AssemblyService : IAssemblyService
    {
        string _pluginsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        Dictionary<string, Assembly> _map;
        Dictionary<string, PluginConfiguration> _cfgMap;

        public AssemblyService()
        {
            _map = new Dictionary<string, Assembly>();
            _cfgMap = new Dictionary<string, PluginConfiguration>();

            Directory.GetFiles(_pluginsFolder).ToList().ForEach(f =>
            {
                var fileName = Path.GetFileName(f).ToLower();
                if (
                fileName != ("processassistant.dll") &&
                (fileName.StartsWith("processassistant.") && fileName.EndsWith(".dll"))
                )
                {
                    var asm = Assembly.LoadFile(f);
                    var typeName = $"{Path.GetFileNameWithoutExtension(f)}.PluginConfigurationService";
                    var ins = asm.CreateInstance(typeName);
                    var asb = ins as IPluginConfigurationService;

                    var pluginConfiguration = asb.GetPluginConfig();

                    _cfgMap.Add(pluginConfiguration.MatchedHostName, pluginConfiguration);
                    _map.Add(pluginConfiguration.SymbolName, asm);
                }
            });
        }

        public PluginConfiguration GetPluginConfiguration(string hostName)
        {
            if (_cfgMap.TryGetValue(hostName, out var cfg))
            {
                return cfg;
            }
            else
            {
                return null;
            }
        }

        public PluginConfiguration GetPluginConfigurationByName(string pluginDisplayName)
        {
            return _cfgMap.FirstOrDefault(k => k.Value.DisplayName.ToLower() == pluginDisplayName.ToLower()).Value;
        }

        public List<PluginConfiguration> GetFoundedPlugins()
        {
            return _cfgMap.Select(k => k.Value).ToList();
        }

        public IProcessAssistant GetProcessAssistant(PluginConfiguration pluginConfiguration)
        {
            try
            {
                var symbol = pluginConfiguration.SymbolName;
                if (_map.TryGetValue(symbol, out var asm))
                {
                    return asm.CreateInstance(symbol) as IProcessAssistant;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                Logger.DefaultLogger.LogError(e, "Get Plugin Process Assistant From Configuration Error");
                return null;
            }
        }
        public IProcessAssistant GetProcessAssistant(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var cfg = GetPluginConfiguration(uri.Host);
                    var symbol = cfg.SymbolName;
                    if (_map.TryGetValue(symbol, out var asm))
                    {
                        return asm.CreateInstance(symbol) as IProcessAssistant;
                    }
                    else
                    {

                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
