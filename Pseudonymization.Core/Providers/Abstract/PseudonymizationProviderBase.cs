using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pseudonymization.Core.Logger;
using Pseudonymization.Core.Pseudonymizers;

namespace Pseudonymization.Core.Providers
{
    public abstract class PseudonymizationProviderBase : IPseudonymizationProvider
    {
        protected readonly string ConnectionString;

        public abstract event ProgressUpdatedEventHandler OnProgressUpdated;
        public abstract event PseudonymizationFailedEventHandler OnPseudonymizationFailed;
        public abstract event PseudonymizationSuccessfulEventHandler OnPseudonymizationSecceeded;

        protected string[] Patterns { get; private set; }

        protected IPseudonymizer Pseudonymizer { get; }
        public ILogger Logger { get; }

        private PseudonymizationProviderBase(IPseudonymizer pseudonymizer, ILogger logger)
        {
            Pseudonymizer = pseudonymizer;
            Logger = logger;
        }

        internal PseudonymizationProviderBase(IPseudonymizer pseudonymizer, string connectionString, ILogger logger)
            : this(pseudonymizer, logger)
        {
            ConnectionString = connectionString;
        }

        public bool Accessible => IsAccessible();

        public abstract Task<IEnumerable<PseudonymizationSchemaRepresentation>> GetPseudonymizationColumnsFromSchemaAsync(CancellationToken cancellationToken);

        public abstract Task PseudonymizeAsync(IEnumerable<PseudonymizationSchemaRepresentation> schemaList);

        internal void LoadPatterns(string[] patterns)
        {
            Patterns = patterns;
        }

        protected abstract bool IsAccessible();

        #region IDisposable
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Pseudonymizer.Dispose();
                }

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
