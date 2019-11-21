using System.Collections.Generic;

namespace Pseudonymization.Core.SchemaAnalysis
{
    public interface ISchemaAnalyzer
    {
        HashSet<Table> GetTablesAndColumns(string schemaName);
        HashSet<string> GetAvailableSchemas();
    }
}
