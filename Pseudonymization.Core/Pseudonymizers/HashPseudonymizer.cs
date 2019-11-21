using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Pseudonymizers
{
    public abstract class HashPseudonymizer : IPseudonymizer
    {
        protected HashAlgorithm HashAlgorithm { get; }

        public HashPseudonymizer(HashAlgorithm hashAlgorithm)
        {
            HashAlgorithm = hashAlgorithm;
        }
        public void Dispose()
        {
            ((IDisposable)HashAlgorithm).Dispose();
        }

        public abstract Task<string> PseudonymizeAsync(string input);
        public abstract string Pseudonymize(string input);
        protected abstract string ComputeHash(string value);

        protected Task<string> ComputeHashAsync(string value)
        {
            return Task.Run(() => ComputeHash(value));
        }
    }
}
