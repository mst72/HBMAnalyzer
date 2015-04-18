using System;
using HBMLogAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var parser = new LogParser();
            var m =
                parser.CheckLogMark(
                    @"2015-01-12 22:40:08,579 [9] DEBUG NHibernate.Loader.Loader - processing result set");
            Assert.AreEqual(true, m.Success);
        }

        [TestMethod]
        public void TestMethodInfo()
        {
            var parser = new LogParser();
            var m =
                parser.CheckLogMark(
                    @"2015-01-12 22:40:05,498 [9] INFO  NHibernate.Loader.Loader - SELECT this_.id as id461_0_, ");
            Assert.AreEqual(true, m.Success);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var m = Regex.Match("2015-01-12 ",
                @"^(?<stamp>\d{4}\-\d{2}\-\d{2})\s+", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            // @"2015-01-12 22:40:08,579 [9] DEBUG NHibernate.Loader.Loader - processing result set"
            Assert.AreEqual(true, m.Success);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var dt = DateTime.ParseExact("2015-01-21 10:59:43,024", "yyyy-MM-dd HH:mm:ss,fff", null);
            var dt2 = DateTime.ParseExact("2015-01-21 11:59:45,924", "yyyy-MM-dd HH:mm:ss,fff", null);
            var res = dt2.Subtract(dt);
            Assert.AreEqual(3602.9, Math.Round(res.TotalSeconds, 1));
            Assert.AreEqual("3602,900", string.Format("{0:##.000}", res.TotalSeconds));
        }

        [TestMethod]
        public void TestMethod4()
        {
            var dt = DateTime.ParseExact("2015-01-21 10:59:43,024", "yyyy-MM-dd HH:mm:ss,fff", null);
            var dt2 = DateTime.ParseExact("2015-01-21 10:59:45,924", "yyyy-MM-dd HH:mm:ss,fff", null);
            var res = dt2.Subtract(dt);
            Assert.AreEqual(2.9, Math.Round(res.TotalSeconds, 1));
            Assert.AreEqual("2,900", string.Format("{0:##.000}", res.TotalSeconds));
        }

    }
}
