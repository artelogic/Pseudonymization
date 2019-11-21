using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Providers
{
    public delegate void ProgressUpdatedEventHandler(object sender, ProgressEventArgs eventArgs);
    public delegate void PseudonymizationSuccessfulEventHandler(object sender, EventArgs eventArgs);
    public delegate void PseudonymizationFailedEventHandler(object sender, EventArgs eventArgs);

    public interface IPseudonymizationProvider : IDisposable
    {
        bool Accessible { get; }
        Task<IEnumerable<PseudonymizationSchemaRepresentation>> GetPseudonymizationColumnsFromSchemaAsync(CancellationToken cancellationToken);
        Task PseudonymizeAsync(IEnumerable<PseudonymizationSchemaRepresentation> schemaList);

        event ProgressUpdatedEventHandler OnProgressUpdated;
        event PseudonymizationFailedEventHandler OnPseudonymizationFailed;
        event PseudonymizationSuccessfulEventHandler OnPseudonymizationSecceeded;
    }
}
