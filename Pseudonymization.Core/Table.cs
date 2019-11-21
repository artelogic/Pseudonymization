using System.Collections.Generic;

namespace Pseudonymization.Core
{
    public class Table
    {
        public string Name { get; set; }

        public long BulkBlocks { get; set; }

        public HashSet<ColumnMetadata> Columns { get; set; }
    }
}
