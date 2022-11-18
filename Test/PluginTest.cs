using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDataCollection.Services;

namespace Test
{
    public class PluginTest
    {

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            AssemblyService ass = new AssemblyService();
            var cfg = ass.GetPluginConfiguration("weibo.com");
            var ins = ass.GetProcessAssistant(cfg.SymbolName);
            var strs = ins.GetAjaxUrlFilters();
        }
    }
}
