using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Logger
{
    public interface ILogger
    {
        //static ILogger Create(string logForder);
        void Log(string data, params object[] args);
        void LogDebug(string data, params object[] args);
        void LogError(Exception e, string data, params object[] args);
    }
}
