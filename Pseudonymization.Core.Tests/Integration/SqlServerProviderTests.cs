using System.Configuration;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pseudonymization.Core.Providers;

namespace Pseudonymization.Core.Tests.Integration
{
    [TestClass]
    public class SqlServerProviderTests
    {
        private string _connStr;

        public SqlServerProviderTests()
        {
            _connStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
        }

        [TestMethod]
        public void TestAccessibility()
        {
            var factory = new PseudonymizationProviderFactory();
            var p = factory.GetProvider(typeof(SqlServerPseudonymizationProvider).FullName, _connStr);
            var accessible = p.Accessible;

            Assert.IsTrue(accessible);
        }

        [TestMethod]
        [Ignore]
        public void TestTablePseudo()
        {
            var factory = new PseudonymizationProviderFactory();

            var p = factory.GetProvider(typeof(SqlServerPseudonymizationProvider).FullName, _connStr);

            //(p as SqlServerPseudonymizationProvider).PseudonymizeTable(
            //    "dbo", 
            //    "Customers", 
            //    new HashSet<ColumnMetadata>() { new ColumnMetadata("ContactEmail") { MaxLength = 128 } });//.GetAwaiter().GetResult();
        }

        [TestMethod]
        [Ignore]
        public void TestFullSqlPseudo()
        {
            var factory = new PseudonymizationProviderFactory();
            var p = factory.GetProvider(typeof(SqlServerPseudonymizationProvider).FullName, _connStr);
            var schemaList = p.GetPseudonymizationColumnsFromSchemaAsync(CancellationToken.None).GetAwaiter().GetResult();

            p.PseudonymizeAsync(schemaList).GetAwaiter().GetResult();
        }
    }
}