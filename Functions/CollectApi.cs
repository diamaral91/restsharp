using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;
using DepositAccount.APITest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Net;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Functions
{
    public class CollectApi
    {

        private readonly IConfiguration _config;
        private readonly RestClient _client;

        public CollectApi()
        {
            _config = Configuration.GetConfigs();

            var baseUrl = _config.GetSection("GatewayUrl").Value;
            _client = new RestClient(baseUrl);
        }
        public async void Collect(int transactionId, double value)
        {
            var request = new RestRequest(_config.GetSection("DailyPayment:Collect:Path").Value, Method.Put);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Ocp-Apim-Subscription-Key", _config.GetSection("Ocp-Apim-Subscription-Key").Value);
            request.AddUrlSegment("managerId", _config.GetSection("UserId").Value);

            var transactions = new[] { new Transactions {
                    Id = transactionId,
                    Value = value
                }
            };

            request.AddJsonBody(new
            {
                transactions,
                ClosingBalance = false,
                ChargeUnpaidBalanceFee = false
            });

            var response = await _client.PutAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
