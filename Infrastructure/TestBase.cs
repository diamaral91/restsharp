using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using RestSharp;
using Xunit.Abstractions;

namespace DepositAccount.APITest.Infrastructure
{
    public class TestBase
    {
        protected readonly IConfiguration _config;
        protected readonly ITestOutputHelper _outputHelper;
        protected readonly CitiFileData _citiFile;
        protected readonly RestClient _client;
        protected readonly AsyncRetryPolicy _policy;

        public TestBase(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _config = Configuration.GetConfigs();
            _client = new RestClient(_config.GetSection("GatewayUrl").Value);
            _citiFile = new CitiFileData("PaymentData");
            _policy = Policy.Handle<Exception>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5));
        }
    }
}
