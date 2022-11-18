using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Newtonsoft.Json.Linq;

namespace Test
{
    public class JPathTest
    {
        string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JPathTestData.json");
        //string xpath = "$.data.list[*].[id,created_at,text_raw,pic_ids,region_name]";
        string xpath = "$.data.list[0].['id','created_at','text_raw','pic_ids','region_name','user.id']";
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            using (var fs = new FileStream(testFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    var data = sr.ReadToEnd();
                    var target = JObject.Parse(data);
                    
                    var p = target.SelectTokens(xpath);
                    if(p != null)
                        Assert.Pass();
                }
            }
        }
    }
}
