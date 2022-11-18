using NUnit.Framework;
using ProcessAssistant.File;

namespace Test
{
    public class Tests
    {
        FileService fs = new FileService();
        string remoteFile = "https://wx3.sinaimg.cn/large/771d5a55ly1h6udavnnz9j20u00r847m.jpg";
        string localFilePath = "D://test.jpg";
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void DownloadFile()
        {
            fs.DownloadFileFromRemote(remoteFile,localFilePath).Wait();
            Assert.Pass();
        }
    }
}