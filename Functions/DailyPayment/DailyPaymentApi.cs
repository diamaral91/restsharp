using DepositAccount.APITest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Net;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Functions.DailyPayment
{
    public class DailyPaymentApi
    {

        private readonly IConfiguration _config;
        private readonly RestClient _client;

        public DailyPaymentApi()
        {
            _config = Configuration.GetConfigs();

            var baseUrl = _config.GetSection("GatewayUrl").Value;
            _client = new RestClient(baseUrl);
        }

        public async void ChangeStatusPost(int transactionId, int status)
        {
            var request = new RestRequest(_config.GetSection("DailyPayment:ChangeTransactionStatus:Path").Value, Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Ocp-Apim-Subscription-Key", _config.GetSection("Ocp-Apim-Subscription-Key").Value);
            request.AddUrlSegment("managerId", _config.GetSection("UserId").Value);

            request.AddJsonBody(new
            {
                TransactionId = transactionId,
                Status = status,
                CloseBalance = false,
                GenerateTransferDiff = false
            });

            var response = await _client.PostAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public async void CreateRequestApprovePut(int transactionId)
        {
            var request = new RestRequest(_config.GetSection("DailyPayment:Approve:Path").Value, Method.Put);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Ocp-Apim-Subscription-Key", _config.GetSection("Ocp-Apim-Subscription-Key").Value);
            request.AddUrlSegment("managerId", _config.GetSection("UserId").Value);

            request.AddBody(new int[] { transactionId } );

            var response = await _client.PutAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
