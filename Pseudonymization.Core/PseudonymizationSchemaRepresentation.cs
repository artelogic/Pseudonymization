using System.Collections.Generic;
using System.Data;

namespace Pseudonymization.Core
{
    public class PseudonymizationSchemaRepresentation
    {
        public string SchemaName { get; set; }

        public HashSet<Table> Tables { get; set; }

        public PseudonymizationSchemaRepresentation(string schemaName, HashSet<Table> tables)
        {
            SchemaName = schemaName;
            Tables = tables;
        }

        public PseudonymizationSchemaRepresentation()
        {
        }
    }
}
