using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.State
{
    public class StateService
    {
        public Dictionary<string, object> _map = new Dictionary<string, object>();

        public bool Add<T>(string name, T data)
        {
            try
            {

                _map.Add(name, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Set<T>(string name, T data)
        {
            try
            {
                _map[name] = data;
                return true;
            }
            catch
            {
                return false;
            }
        }


        public T Get<T>(string name)
        {
            if (_map.TryGetValue(name, out var data))
            {
                return (T)data;
            }
            return default(T);
        }

    }
}
