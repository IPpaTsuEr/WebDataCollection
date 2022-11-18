using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.History
{
    /// <summary>
    /// 此次运行中已处理过的对象记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HistoryManager<T> : IHistoryManager
    {
        private static Dictionary<T, bool> _map = new Dictionary<T, bool>();

        public HistoryManager()
        {

        }

        public bool IsInHistory(T targetId)
        {
            return _map.ContainsKey(targetId);
        }

        public bool AddToHistory(T target)
        {
            try
            {
                _map.Add(target, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool LoadFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                var tmap = Load<Dictionary<T, bool>>(filePath);
                foreach (var item in tmap)
                {
                    if (!_map.ContainsKey(item.Key))
                    {
                        _map.Add(item.Key, item.Value);
                    }
                }
                return true;
            }
            return false;
        }
        public bool SaveToFile(string filePath)
        {
            Save(_map, filePath);
            return true;
        }

        private E Load<E>(string filePath)
        {
            string data = string.Empty;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sm = new StreamReader(fs, Encoding.UTF8))
                {
                    data = sm.ReadToEnd();
                }
            }
            return JsonConvert.DeserializeObject<E>(data);
        }

        private void Save<E>(E data, string filePath)
        {
            string dataStr = JsonConvert.SerializeObject(data);
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var sm = new StreamWriter(fs, Encoding.UTF8))
                {
                    sm.Write(dataStr);
                }
            }
        }

        public bool AddToHistory(object key)
        {
            return AddToHistory((T)key);
        }
    }
}
