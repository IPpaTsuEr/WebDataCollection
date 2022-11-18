using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.History
{
    public interface IHistoryManager
    {
        bool AddToHistory(object key);
    }
}
