using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pseudonymization.Core.ConnectionAnalysis;
using Pseudonymization.Core.Exceptions;
using Pseudonymization.Core.Providers;

namespace Pseudonymization.Core.Tests.Unit
{
    [TestClass]
    public class SqlConnectionAnalyzerTest
    {
        SqlServerConnectionAnalyzer _connectionAnalyzer = new SqlServerConnectionAnalyzer();

        public SqlConnectionAnalyzerTest()
        {
            
        }

        [TestMethod]
        public void SqlServerConnectionAnalyzer_TestSplitConnectionBlocks_ReturnsSetOfConnectionKeysAndValues()
        {
            var pair = new KeyValuePair<string, string>("Initial Catalog", "SampleDatabase");
            var connectionString = "Initial Catalog=SampleDatabase";

            var result = _connectionAnalyzer.SplitConnectionBlock(connectionString);

            Assert.AreEqual(pair, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidConnectionBlockException))]
        public void SqlServerConnectionAnalyzer_TestSplitConnectionBlocks_ThrowsInvalidConnectionBlock()
        {
            var pair = new KeyValuePair<string, string>("initial catalog", "sampledatabase");
            var connectionString = "Initial Catalog:SampleDatabase";

            var result = _connectionAnalyzer.SplitConnectionBlock(connectionString);
        }

        [TestMethod]
        public void ConnectionAnalyzer_TestLookupOnValidConnectionParts_ReturnsTrue()
        {
            //var config = new ConnectionStringMatchConfig();

            //var connectionStringParts = new[]
            //{
            //    new KeyValuePair<string, string>("initial catalog", "SampleDatabase"),
            //    new KeyValuePair<string, string>("data source", "XXX.YYY.WW.ZZ")
            //};

            //bool result = _connectionAnalyzer.Lookup(config, connectionStringParts);

            //Assert.IsTrue(result);
        }

        [TestMethod]
        public void SqlServerConnectionAnalyzer_TestIsMatch_ReturnsTrue()
        {
            //var validConfig = new List<ConnectionStringMatchConfig>();

            //var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ToString();

            //bool match = _connectionAnalyzer.IsMatch(connectionString, validConfig);

            //Assert.IsTrue(match);
        }

        [TestMethod]
        [Ignore]
        public void SqlServerConnectionAnalyzer_TestIsMatch_OnMySqlConnectionStringReturnsFalse()
        {
            //var validConfig = new List<ConnectionStringMatchConfig>()
            //{

            //};

            var connectionString = "Initial Catalog=SampleDatabase;Data Source=XXX.YYY.WW.ZZ;Protocol=socket";

            //bool match = _connectionAnalyzer.IsMatch(connectionString, validConfig);

            //Assert.IsFalse(match);
        }
    }
}
