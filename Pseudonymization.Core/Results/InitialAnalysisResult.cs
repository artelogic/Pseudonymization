using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Results
{
    public class InitialAnalysisResult<TSet>
    {
        public string ProviderName { get; set; }
        public TSet AnalysisData { get; set; }
    }
}
