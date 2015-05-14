using HBMLogAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace UnitTestProject1
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
        ///A test for RecognizeOperation
        ///</summary>
        [TestMethod()]
        public void RecognizeOperationTest()
        {
            string chunk = @"INSERT INTO table1 (date, p_id, p_id, id) VALUES (:p0, :p1, :p2, :p3);:p0 = 2015-01-15 10:41:06 [Type: DateTime (0)], :p1 = 38931 [Type: Int64 (0)], :p2 = 5151165 [Type: Int64 (0)], :p3 = 30472839 [Type: Int64 (0)]"; 
            var actual = LogParserHelper.RecognizeOperation(chunk);
            Assert.AreEqual(Operations.Insert, actual);
        }
        [TestMethod()]
        public void RecognizeOperationTest2()
        {
            string chunk = @"select";
            var actual = LogParserHelper.RecognizeOperation(chunk);
            Assert.AreEqual(Operations.Select, actual);
        }

    }
}
