using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProcessAssistant.File
{
    public class FileService : IFileService
    {

        public T LoadFromFile<T>(string filePath)
        {
            string data = string.Empty;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sm = new StreamReader(fs, Encoding.UTF8))
                {
                    data = sm.ReadToEnd();
                }
            }
            return JsonConvert.DeserializeObject<T>(data);
        }

        public void SaveToFile<T>(T data, string filePath)
        {
            try
            {

                string dataStr = JsonConvert.SerializeObject(data);
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var sm = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sm.Write(dataStr);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Logger.DefaultLogger?.LogError(e, "保存数据[{0}]到[{1}]失败", data, filePath);
            }
        }
        public void SaveStringDataToFile(string data, string filePath)
        {
            try
            {

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var sm = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sm.Write(data);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Logger.DefaultLogger?.LogError(e, "下载数据[{0}]到[{1}]失败", data, filePath);
            }
        }
        public string LoadStringDataFromFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sm = new StreamReader(fs, Encoding.UTF8))
                {
                    return sm.ReadToEnd();
                }
            }
        }

        public Task DownloadFileFromRemote(string url, string savePathName, string profix)
        {
            try
            {
                WebClient wc = new WebClient();
                if (!string.IsNullOrWhiteSpace(profix))
                {
                    var newName = $"{profix}_{Path.GetFileName(savePathName)}";
                    var folder = Path.GetDirectoryName(savePathName);
                    savePathName = Path.Combine(folder, newName);
                }
                return wc.DownloadFileTaskAsync(url, savePathName);

            }
            catch (Exception e)
            {
                Logger.Logger.DefaultLogger?.LogError(e, "下载文件[{0}]到[{1}]失败", url, savePathName);
                return Task.FromException(e);
            }
        }
        public Task<byte[]> DownloadDataFromRemote(string url)
        {
            try
            {
                WebClient wc = new WebClient();
                return wc.DownloadDataTaskAsync(url);
            }
            catch
            {
                return null;
            }
        }

        public bool IsFileExists(string path)
        {
            return System.IO.File.Exists(path);
        }
    }
}
