using System;

namespace Pseudonymization.Core.Exceptions
{
    public class SchemaAnalysisFailedException : Exception
    {
        private SchemaAnalysisFailedException()
        {
        }

        public SchemaAnalysisFailedException(string message)
            : base(message)
        {
        }

        public SchemaAnalysisFailedException(string schemaName, Exception original)
            : base($"Schema analysis failed for {schemaName} with message: {original.Message}", original)
        {
        }
    }
}
