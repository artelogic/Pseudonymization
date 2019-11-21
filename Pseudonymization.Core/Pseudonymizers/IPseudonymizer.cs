using System;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Pseudonymizers
{
    // provides contractsfor different pseudonymizers
    public interface IPseudonymizer : IDisposable
    {
        Task<string> PseudonymizeAsync(string input);
        string Pseudonymize(string input);
    }
}
