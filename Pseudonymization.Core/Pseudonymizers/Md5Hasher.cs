using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pseudonymization.Core.Pseudonymizers
{
    public class Md5HashPseudonymizer : HashPseudonymizer
    {
        public Md5HashPseudonymizer() : base(MD5.Create())
        {
        }

        // can perform additional tasks with tokenization etc.
        public override Task<string> PseudonymizeAsync(string input)
        {
            return ComputeHashAsync(input);
        }

        public override string Pseudonymize(string input)
        {
            return ComputeHash(input);
        }

        protected override string ComputeHash(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var builder = new StringBuilder();

            foreach (var @byte in HashAlgorithm.ComputeHash(bytes))
            {
                builder.Append(@byte.ToString("X2"));
            }

            return builder.ToString().ToUpper();
        }
    }
}
