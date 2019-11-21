using System;

namespace Pseudonymization.Core.Attributes
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ProviderAttribute : Attribute
    {
        public ProviderAttribute()
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Type ConnectionAnalyzerType { get; set; }
    }
}
