using Microsoft.Extensions.Configuration;
using System;

namespace TeamsConfiguration.Services
{
    public class ConfigurationService
    {
        private ConfigurationService(string appDirectory)
        {
            if (string.IsNullOrEmpty(appDirectory))
                throw new ArgumentNullException(nameof(appDirectory));
            Configuration = new Lazy<IConfiguration>(() =>
            {
                return new ConfigurationBuilder()
                    .SetBasePath(appDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            });
        }
        public Lazy<IConfiguration> Configuration { get; private set; }
        private static ConfigurationService _instance;
        public static ConfigurationService GetInstance(string appDirectory)
        {
            if(_instance == null)
                _instance = new ConfigurationService(appDirectory);
            return _instance;
        }
    }
}
