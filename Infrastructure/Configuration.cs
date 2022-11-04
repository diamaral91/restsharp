using Microsoft.Extensions.Configuration;

namespace DepositAccount.APITest.Infrastructure
{
    public class Configuration
    {
        public static IConfigurationRoot GetConfigs()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
