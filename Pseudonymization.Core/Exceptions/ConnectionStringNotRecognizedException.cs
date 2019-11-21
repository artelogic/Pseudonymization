using System;

namespace Pseudonymization.Core.Exceptions
{
    public class ConnectionStringNotRecognizedException : Exception
    {
        public ConnectionStringNotRecognizedException() : base("Connection string was not recognized or system could not found connection provider")
        {

        }
    }
}
