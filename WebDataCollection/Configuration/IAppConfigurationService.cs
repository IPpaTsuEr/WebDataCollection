using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Model.CollectionTask;

namespace WebDataCollection.Configuration
{
    public interface IAppConfigurationService
    {
        AppConfiguration ApplacationConfiguration { get; set; }
        TaskExcuteConfiguration GetTaskConfiguration(string url);
        TaskExcuteConfiguration GetTaskConfigurationByName(string name);
    }
}
