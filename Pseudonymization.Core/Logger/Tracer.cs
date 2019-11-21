using System;
using System.Diagnostics;

namespace Pseudonymization.Core.Logger
{
    public class Tracer : ILogger
    {
        public void Log(Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }

        public void Log(string message)
        {
            Trace.WriteLine(message);
        }
    }
}
