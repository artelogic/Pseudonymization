using System;
using System.Collections.Generic;
using System.Linq;

namespace Pseudonymization.Core.Logger
{
    public class LogHolder : List<ILogger>, ILogger
    {
        public LogHolder()
        {
        }

        public LogHolder(IEnumerable<ILogger> loggers)
            :base(loggers)
        {
        }

        public void Log(Exception ex)
        {
            this.AsParallel().ForAll(l => l.Log(ex));
        }

        public void AddLogger(ILogger logger)
        {
            this.Add(logger);
        }

        public void RemoveLogger(ILogger logger)
        {
            this.Remove(logger);
        }

        public void Log(string message)
        {
            this.AsParallel().ForAll(l => l.Log(message));
        }
    }
}
