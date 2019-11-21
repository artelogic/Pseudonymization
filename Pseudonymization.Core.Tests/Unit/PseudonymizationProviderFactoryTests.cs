using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pseudonymization.Core.Exceptions;
using Pseudonymization.Core.Providers;

namespace Pseudonymization.Core.Tests.Unit
{
    [TestClass]
    public class PseudonymizationProviderFactoryTests
    {
        private string connStr;

        public PseudonymizationProviderFactoryTests()
        {
            connStr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
        }

        [TestMethod]
        public void PseudonymizationProviderFactory_GetPseudonymizationColumnsFromSchemaAsync_ThrowsNoExceptions()
        {
            var factory = new PseudonymizationProviderFactory();

            var p = factory.GetProvider(typeof(SqlServerPseudonymizationProvider).FullName, connStr);
            p.GetPseudonymizationColumnsFromSchemaAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void PseudonymizationProviderFactory_GetAvailableProviders_ReturnsListOfProviders()
        {
            var factory = new PseudonymizationProviderFactory();
            var avPrs = factory.GetAvailableProviders();

            bool providersExist = avPrs.Any();

            Assert.IsTrue(providersExist);
        }

        [TestMethod]
        public void PseudonymizationProviderFactory_GetProviderByName_ReturnsProvider()
        {
            var factory = new PseudonymizationProviderFactory();
            var provider = factory.GetProvider(typeof(SqlServerPseudonymizationProvider).FullName, connStr);

            Assert.IsNotNull(provider);
            Assert.IsInstanceOfType(provider, typeof(SqlServerPseudonymizationProvider));
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderInitializationFailedException))]
        public void PseudonymizationProviderFactory_GetProviderByName_ThrowsProviderInitializationFailedException()
        {
            var factory = new PseudonymizationProviderFactory();
            var provider = factory.GetProvider(typeof(PseudonymizationProviderFactory).FullName, connStr);
        }

        [TestMethod]
        public void PseudonymizationProviderFactory_GetProviderByName_ThrowsInnerProviderNotFoundException()
        {
            var factory = new PseudonymizationProviderFactory();
            try
            {
                var provider = factory.GetProvider(typeof(PseudonymizationProviderFactory).FullName, connStr);
            }
            catch (ProviderInitializationFailedException ex)
            when (ex.InnerException is ProviderNotFoundException)
            {
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }
    }
}
