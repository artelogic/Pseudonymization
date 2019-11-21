using System.Configuration;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pseudonymization.Core.Providers;
using Pseudonymization.Core.SchemaAnalysis;

namespace Pseudonymization.Core.Tests.Integration
{
    [TestClass]
    public class TestSqlServerAnalyzer
    {
        private string _connStr;

        public TestSqlServerAnalyzer()
        {
            _connStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
        }

        [TestMethod]
        [Ignore]
        public void TestSchemaAnalysis()
        {
            var conn = new SqlConnection(_connStr);

            var fact = new PseudonymizationProviderFactory();

            var t = new SqlServerSchemaAnalyzer(conn, fact.GetTriggerPatterns());
            
            var dt = t.GetTablesAndColumns("dbo");
        }
    }
}
