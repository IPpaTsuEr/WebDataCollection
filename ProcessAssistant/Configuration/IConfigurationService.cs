using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Configuration
{
    public interface IConfigurationService
    {
        T Load<T>(string file);
        bool Save<T>(string file,T ConfigData);


    }
}
