using DepositAccount.APITest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Net;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Functions.BankTransfers
{
    public class BankTransferApi
    {
        private readonly IConfiguration _config;
        private readonly RestClient _client;

        public BankTransferApi()
        {
            _config = Configuration.GetConfigs();

            var baseUrl = _config.GetSection("BankTransfers:BaseUrl").Value;
            _client = new RestClient(baseUrl);
        }

        public async void PostFakeBank(Guid transferId, int status)
        {
            var request = CreateFakeBankRequest(transferId, status);
            var response = await _client.PostAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private RestRequest CreateFakeBankRequest(Guid transferId, int status)
        {
            var request = new RestRequest(_config.GetSection("BankTransfers:Path").Value, Method.Put);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Ocp-Apim-Subscription-Key", _config.GetSection("BankTransfers:Ocp-Apim-Subscription-Key").Value);
            request.AddQueryParameter("Code", _config.GetSection("BankTransfers:Code").Value);

            request.AddJsonBody(new
            {
                TransferId = transferId,
                Status = status
            });

            return request;
        }
    }
}
