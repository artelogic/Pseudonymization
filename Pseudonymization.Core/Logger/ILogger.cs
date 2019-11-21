using System;

namespace Pseudonymization.Core.Logger
{
    public interface ILogger
    {
        void Log(Exception ex);
        void Log(string message);
    }
}
