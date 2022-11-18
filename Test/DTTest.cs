using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class DTTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void DateTImeConverter()
        {
            var datastring = "Tue Sep 27 19:53:25 +0800 2022";

            var date = DateTime.ParseExact(datastring, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.CreateSpecificCulture("en-US"));
            Console.WriteLine(date.ToString("yyyy-MM-dd HH:mm:ss dddd MMMM zzz"));
        }
    }
}
