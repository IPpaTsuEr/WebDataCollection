using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.Logger
{
    public class Logger : ILogger, IDisposable
    {
        public static ILogger DefaultLogger { get; set; }

        private FileStream fileStream;
        private StreamWriter streamWriter;
        private Logger(string logForder)
        {
            if (!Directory.Exists(logForder))
            {
                Directory.CreateDirectory(logForder);
            }
            var filePath = Path.Combine(logForder,$"{DateTime.Now:yyyy-MM-dd}.log");

            fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            streamWriter = new StreamWriter(fileStream);
        }

        public static ILogger Create(string logForder)
        {
            return new Logger(logForder);
        }

        public void Log(string data, params object[] args)
        {
            if (args is null)
            {
                streamWriter.WriteLine(data);
                Console.WriteLine(data);
            }
            else
            {
                streamWriter.WriteLine(string.Format(data, args));
                Console.WriteLine(string.Format(data, args));
            }
            streamWriter.Flush();
        }
        public void LogDebug(string data, params object[] args)
        {
            Log($"[Debug] {data}", args);
        }
        public void LogError(Exception e, string data, params object[] args)
        {

            Log($"[Error] {data} {e.ToString()}", args);
        }

        public void Dispose()
        {
            streamWriter?.Close();
            fileStream?.Close();
        }
    }
}
