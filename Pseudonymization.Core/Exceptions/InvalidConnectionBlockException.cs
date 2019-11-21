using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Exceptions
{
    public class InvalidConnectionBlockException : Exception
    {
        public InvalidConnectionBlockException(string message) : base(message)
        {
        }
    }
}
