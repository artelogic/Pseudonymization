using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudonymization.Core
{
    public class ColumnMetadata
    {
        public string ColumnName { get; set; }
        public int? MaxLength { get; set; }

        public ColumnMetadata(string columnName)
        {
            ColumnName = columnName;
        }

        public ColumnMetadata()
        {
        }
    }
}
