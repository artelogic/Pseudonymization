using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Pseudonymization.Core.ConnectionAnalysis;
using Pseudonymization.Core.Exceptions;
using Pseudonymization.Core.Logger;
using Pseudonymization.Core.Pseudonymizers;

namespace Pseudonymization.Core.Providers
{
    public class PseudonymizationProviderFactory
    {
        private readonly ILogger _logger;

        public PseudonymizationProviderFactory()
        {
            var logHolder = new LogHolder();
            logHolder.AddLogger(new Tracer());
            _logger = logHolder;
        }

        public IPseudonymizationProvider GetProvider(Type providerType, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            if (providerType == null)
            {
                throw new ArgumentNullException(nameof(providerType));
            }

            if (!typeof(PseudonymizationProviderBase).IsAssignableFrom(providerType))
            {
                throw new ProviderNotFoundException(providerType.FullName);
            }

            var providerConstructor = providerType.GetConstructor(new[] { typeof(IPseudonymizer), typeof(string), typeof(ILogger) });

            if (providerConstructor == null)
            {
                throw new Exception("No constructor found to create a type with factory parameters: pseudonymizer, connectionString, logger");
            }

            // bad approach
            var instance = providerConstructor.Invoke(new[] { (object)GetDefaultPseudonymizer(), (object)connectionString, (object)_logger }) as PseudonymizationProviderBase;
            instance.LoadPatterns(GetTriggerPatterns());
            return instance as IPseudonymizationProvider;
        }

        public IPseudonymizationProvider GetProvider(string providerName, string connectionString)
        {
            try
            {
                var providerType = Assembly.GetExecutingAssembly().GetType(providerName);
                return GetProvider(providerType, connectionString);
            }

            catch (Exception ex)
            {
                throw new ProviderInitializationFailedException(providerName, ex);
            }
        }

        public IPseudonymizer GetDefaultPseudonymizer()
        {
            return new Md5HashPseudonymizer();
        }

        public IEnumerable<Type> GetAvailableProviders()
        {
            return Assembly
                .GetExecutingAssembly()
                .ExportedTypes
                .Where(type => typeof(PseudonymizationProviderBase).IsAssignableFrom(type) && !type.IsAbstract)
                .ToArray();
        }

        public string[] GetTriggerPatterns()
        {
            try
            {
                // todo: move to config name of file
                return GetConfigValuesFromFile<string>("trigger-column-patterns.json").ToArray();
            }
            catch(Exception ex)
            {
                return new string[0];
            }
        }

        private IEnumerable<TType> GetConfigValuesFromFile<TType>(string requestedFileName)
        {
            string fileName = $"{AppDomain.CurrentDomain.RelativeSearchPath}\\{requestedFileName}";
            string text = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<IEnumerable<TType>>(text);
        }
    }
}