using PostgresLogAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace Test.PostgresLogAnalyzer
{
    
    
    /// <summary>
    ///This is a test class for LogParserTest and is intended
    ///to contain all LogParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LogParserTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for CheckLogMark
        ///</summary>
        [TestMethod()]
        public void CheckLogMarkTest()
        {
            var target = new LogParser();
            var actual = target.CheckLogMark(@"2015-04-09 16:00:03 EEST:                    :     @   :[4093] : [25293-1]: LOG:  ");
            Assert.AreEqual(true, actual.Success);
            Assert.AreEqual("2015-04-09 16:00:03", actual.Groups["stamp"].ToString());
            Assert.AreEqual("EEST", actual.Groups["timezone"].ToString());
            Assert.AreEqual("", actual.Groups["ip"].ToString());
            Assert.AreEqual("@", actual.Groups["login"].ToString().Trim());
            Assert.AreEqual("4093", actual.Groups["id1"].ToString());
            Assert.AreEqual("25293-1", actual.Groups["id2"].ToString());
            Assert.AreEqual("LOG", actual.Groups["kind"].ToString());
            Assert.AreEqual("  ", actual.Groups["chunk"].ToString());
        }
         
         

        [TestMethod()]
        public void CheckLogMarkTest2()
        {
            var target = new LogParser();
            var actual = target.CheckLogMark(@"2015-04-09 16:23:21 EEST:                    :     @   :[18582]: [2-1]: FATAL:");
            Assert.AreEqual(true, actual.Success);
            Assert.AreEqual("2015-04-09 16:23:21", actual.Groups["stamp"].ToString());
            Assert.AreEqual("EEST", actual.Groups["timezone"].ToString());
            Assert.AreEqual("", actual.Groups["ip"].ToString());
            Assert.AreEqual("@", actual.Groups["login"].ToString().Trim());
            Assert.AreEqual("18582", actual.Groups["id1"].ToString());
            Assert.AreEqual("2-1", actual.Groups["id2"].ToString());
            Assert.AreEqual("FATAL", actual.Groups["kind"].ToString());
            Assert.AreEqual("", actual.Groups["chunk"].ToString());
        }

        [TestMethod()]
        public void CheckLogMarkTest3()
        {
            var target = new LogParser();
            var actual = target.CheckLogMark(@"2015-04-09 16:24:54 EEST:192.168.10.16(54238):TaMo@TaMo:[43856]: [1-1]: ERROR: ");
            Assert.AreEqual(true, actual.Success);
            Assert.AreEqual("2015-04-09 16:24:54", actual.Groups["stamp"].ToString());
            Assert.AreEqual("EEST", actual.Groups["timezone"].ToString());
            Assert.AreEqual("192.168.10.16(54238)", actual.Groups["ip"].ToString());
            Assert.AreEqual("TaMo@TaMo", actual.Groups["login"].ToString().Trim());
            Assert.AreEqual("43856", actual.Groups["id1"].ToString());
            Assert.AreEqual("1-1", actual.Groups["id2"].ToString());
            Assert.AreEqual("ERROR", actual.Groups["kind"].ToString());
            Assert.AreEqual(" ", actual.Groups["chunk"].ToString());
        }
        
        [TestMethod()]
        public void CheckLogMarkTest4()
        {
            var target = new LogParser();
            var actual = target.CheckLogMark(@"2015-04-09 16:00:03 EEST::@:[4093]:[25293-1]: LOG:  restartpoint starting: time");
            Assert.AreEqual(true, actual.Success);
            Assert.AreEqual("2015-04-09 16:00:03", actual.Groups["stamp"].ToString());
            Assert.AreEqual("EEST", actual.Groups["timezone"].ToString());
            Assert.AreEqual("", actual.Groups["ip"].ToString());
            Assert.AreEqual("@", actual.Groups["login"].ToString().Trim());
            Assert.AreEqual("4093", actual.Groups["id1"].ToString());
            Assert.AreEqual("25293-1", actual.Groups["id2"].ToString());
            Assert.AreEqual("LOG", actual.Groups["kind"].ToString());
            Assert.AreEqual("  restartpoint starting: time", actual.Groups["chunk"].ToString());
        }

        /// <summary>
        ///A test for CheckLogMark2
        ///</summary>
        [TestMethod()]
        public void CheckLogMark2Test()
        {
            var target = new LogParser();
            var actual = target.CheckLogMark2(@"  duration: 1747.419 ms  statement: SELECT count(*) FROM pg_class");
            Assert.AreEqual(true, actual.Success);
            Assert.AreEqual("1747.419", actual.Groups["duration_val"].ToString());
            Assert.AreEqual("ms", actual.Groups["duration_suffix"].ToString());
            Assert.AreEqual("SELECT count(*) FROM pg_class", actual.Groups["statement"].ToString());
        }

        /// <summary>
        ///A test for CheckLogMark2
        ///</summary>
        [TestMethod()]
        public void CheckLogMark3Test()
        {
            var target = new LogParser();
            var actual = target.CheckLogMark2(@"duration: 1605.489 ms  statement: with periodu_datos as
  select
");
            Assert.AreEqual(true, actual.Success);
            Assert.AreEqual("1605.489", actual.Groups["duration_val"].ToString());
            Assert.AreEqual("ms", actual.Groups["duration_suffix"].ToString());
            var res = actual.Groups["statement"].ToString();
            Assert.AreEqual("with periodu_datos as\r\n  select\r\n", actual.Groups["statement"].ToString());
        }
    }
}
