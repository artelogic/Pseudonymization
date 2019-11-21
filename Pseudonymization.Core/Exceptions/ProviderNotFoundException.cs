using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Exceptions
{
    public class ProviderNotFoundException : Exception
    {
        private ProviderNotFoundException()
        {
        }

        public ProviderNotFoundException(string providerName)
            : base($"Provider {providerName} was not found")
        {

        }
    }
}
