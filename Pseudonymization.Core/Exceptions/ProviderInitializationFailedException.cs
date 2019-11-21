using System;

namespace Pseudonymization.Core.Exceptions
{
    public class ProviderInitializationFailedException : Exception
    {
        private ProviderInitializationFailedException()
        {
        }

        public ProviderInitializationFailedException(string providerName, Exception original)
            :base($"Pseudonymization provider lookup failed for {providerName} with message: {original.Message}. For further details see inner exception", original)
        {
        }
    }
}