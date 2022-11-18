using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Configuration
{
    public class PluginConfiguration
    {
        public string SymbolName { get; set; }
        public string DisplayName { get; set; }
        public string MatchedHostName { get; set; }
        public string WorkSpaceRoot { get; set; }
        public Version Version { get; set; }

    }
}
