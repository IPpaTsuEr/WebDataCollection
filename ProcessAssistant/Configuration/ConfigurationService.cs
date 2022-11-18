using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public T Load<T>(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var sm = new StreamReader(fs,Encoding.UTF8))
                {
                    var data = sm.ReadToEnd();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
                }
            }
        }

        public bool Save<T>(string file, T configData)
        {
            using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var sw = new StreamWriter(fs,Encoding.UTF8))
                {
                    var data = Newtonsoft.Json.JsonConvert.SerializeObject(configData);
                    sw.Write(data);
                    return true;
                }
            }
        }
    }
}
