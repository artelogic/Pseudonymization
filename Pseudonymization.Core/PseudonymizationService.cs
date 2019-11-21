using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pseudonymization.Core.Attributes;
using Pseudonymization.Core.ConnectionAnalysis;
using Pseudonymization.Core.Exceptions;
using Pseudonymization.Core.Providers;
using Pseudonymization.Core.Results;

namespace Pseudonymization.Core
{
    public class PseudonymizationService : IDisposable
    {
        private PseudonymizationProviderFactory _providerFactory = new PseudonymizationProviderFactory();
        private IPseudonymizationProvider _provider;
        private ProgressUpdatedEventHandler _handler;
        private PseudonymizationSuccessfulEventHandler _successHandler;
        private PseudonymizationFailedEventHandler _failureHandler;


        public PseudonymizationService()
        {
        }

        public IPseudonymizationProvider Provider
        {
            get => _provider;
            private set => SetProvider(value);
        }

        public async Task Pseudonymize(string connectionString, string providerName, IEnumerable<PseudonymizationSchemaRepresentation> schemaList)
        {
            Provider = _providerFactory.GetProvider(providerName, connectionString);
            await Provider.PseudonymizeAsync(schemaList);
        }

        public async Task<InitialAnalysisResult<IEnumerable<PseudonymizationSchemaRepresentation>>> GetSchemaList(string connectionString, CancellationToken cancellationToken)
        {
            var analyzer = GetAnalyzerForConnection(connectionString);

            if (analyzer == null)
            {
                throw new ConnectionStringNotRecognizedException();
            }

            using (var provider = GetProvider(analyzer, connectionString))
            {
                if (!provider.Accessible)
                {
                    throw new DataSourceNotAccessibleException();
                }

                var data = provider.GetPseudonymizationColumnsFromSchemaAsync(cancellationToken);

                return new InitialAnalysisResult<IEnumerable<PseudonymizationSchemaRepresentation>>()
                {
                    ProviderName = provider.GetType().FullName,
                    AnalysisData = await data
                };
            }
        }

        public void OnProgressUpdated(ProgressUpdatedEventHandler handler)
        {
            _handler = handler;
        }

        public void OnSucceeded(PseudonymizationSuccessfulEventHandler handler)
        {
            _successHandler = handler;
        }

        public void OnFailed(PseudonymizationFailedEventHandler handler)
        {
            _failureHandler = handler;
        }

        private IConnectionStringAnalyzer GetAnalyzerForConnection(string connectionString)
        {
            return GetAvailableanaylzers()
                .Select(analyzerType => Activator.CreateInstance(analyzerType) as IConnectionStringAnalyzer)
                .FirstOrDefault(a => a.IsMatch(connectionString));
        }

        private Type[] GetAvailableanaylzers()
        {
            return Assembly
                .GetExecutingAssembly()
                .DefinedTypes
                .Where(t => typeof(IConnectionStringAnalyzer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToArray();
        }

        private IPseudonymizationProvider GetProvider(IConnectionStringAnalyzer connectionStringAnalyzer, string connectionString)
        {
            var providerType = _providerFactory
                .GetAvailableProviders()
                .Select(p => new
                {
                    provider = p,
                    metadata = p.GetCustomAttribute<ProviderAttribute>()
                })
                .FirstOrDefault(p => p.metadata.ConnectionAnalyzerType == connectionStringAnalyzer.GetType());

            return _providerFactory.GetProvider(providerType.provider, connectionString);
        }

        private void SetProvider(IPseudonymizationProvider value)
        {
            // setup new
            _provider = value;
            _provider.OnProgressUpdated += _handler;
            _provider.OnPseudonymizationFailed += _failureHandler;
            _provider.OnPseudonymizationSecceeded += _successHandler;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _provider?.Dispose();
                }

                _handler = null;
                _successHandler = null;
                _failureHandler = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
