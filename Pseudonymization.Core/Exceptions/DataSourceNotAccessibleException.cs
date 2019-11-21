using System;

namespace Pseudonymization.Core.Exceptions
{
    public class DataSourceNotAccessibleException : Exception
    {
        public DataSourceNotAccessibleException()
            :base("Data Source is not accessible")
        {
        }
    }
}
