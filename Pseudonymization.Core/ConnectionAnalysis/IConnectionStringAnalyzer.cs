using System.Collections.Generic;

namespace Pseudonymization.Core.ConnectionAnalysis
{
    public interface IConnectionStringAnalyzer
    {
        bool IsMatch(string connectionString);
    }
}
