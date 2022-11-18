using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.File
{
    public interface  IFileService
    {
        bool IsFileExists(string path);
        T LoadFromFile<T>(string filePath);
        void SaveToFile<T>(T data, string filePath);
        void SaveStringDataToFile(string data, string filePath);
        string LoadStringDataFromFile(string filePath);
        Task DownloadFileFromRemote(string url, string savePathName,string profix="");
        Task<byte[]> DownloadDataFromRemote(string url);

    }
}
