using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Handles;

namespace WebDataCollection.Services
{
    public class ResponseDataService
    {
        private static Dictionary<ulong, ResponseFilter> _map;

        static ResponseDataService()
        {
            _map = new Dictionary<ulong, ResponseFilter>();
        }

        public static ResponseFilter Get(ulong id)
        {
            if (_map.TryGetValue(id, out var rp))
                return rp;
            return null;
        }
        public static void Remove(ulong id)
        {
            _map.Remove(id);
        }

        public static void Add(ulong id, ResponseFilter sm)
        {
            _map.Add(id, sm);
        }
    }
}
