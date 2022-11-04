using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Enums;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;
using DepositAccount.APITest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RestSharp;
using SellersFunding.PaymentOrchestration.Test.APITest.Domain.Enums;
using System.Net;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Functions.DepositAccount
{
    public class DepositAccountApi
    {
        private readonly IConfiguration _config;
        private readonly RestClient _client;
        private readonly CitiFileData _citiFile;
        public DepositAccountApi(CitiFileData data)
        {
            _config = Configuration.GetConfigs();

            var baseUrl = _config.GetSection("DepositAccount:BaseUrl").Value;
            _client = new RestClient(baseUrl);

            _citiFile = data;
        }

        public async Task<IEnumerable<ReprocessCitiFileDto>> CreateCitiBankTransaction()
        {
            var request = CreateCitiTransactionRequest();
            var response = await _client.PostAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            return _client.Deserialize<IEnumerable<ReprocessCitiFileDto>>(response).Data;
        }

        public RestRequest CreateApproveRequest(Guid transactionId, EMarketplace mktplace)
        {
            var request = new RestRequest(_config.GetSection("DepositAccount:TransactionApprove:Path").Value, Method.Post);

            request.AddHeader("Ocp-Apim-Subscription-Key", _config.GetSection("Ocp-Apim-Subscription-Key").Value);
            request.AddHeader("Cache-Control", "no-cache");

            request.AddJsonBody(new
            {
                TransactionId = transactionId.ToString(),
                Comment = "automationProject",
                SenderName = "Amazon",
                MarketPlaceId = mktplace.ToDescriptionString()
            });
            return request;
        }

        public RestRequest ChangeCompany(Guid transactionId)
        {
            var request = new RestRequest(_config.GetSection("DepositAccount:ChangeCompany:Path").Value, Method.Post);

            request.AddHeader("Ocp-Apim-Subscription-Key", _config.GetSection("Ocp-Apim-Subscription-Key").Value);
            request.AddHeader("Cache-Control", "no-cache");

            request.AddJsonBody(new
            {
                TransactionId = transactionId,
                CompanyId = int.Parse(_citiFile.CompanyId),
                Comment = "apiTest"
            });
            return request;
        }

        private RestRequest CreateCitiTransactionRequest()
        {
            var request = new RestRequest(_config.GetSection("DepositAccount:ReprocessCitiFile:Path").Value, Method.Post);

            request.AddQueryParameter("code", _config.GetSection("DepositAccount:Code").Value);

            if (_citiFile.Currency.Equals("GBP"))
            {
                request.AddStringBody(RequestBodyConstruction(_citiFile), DataFormat.Json);
            }
            else if (_citiFile.Currency.Equals("USD"))
            {
                request.AddStringBody(RequestBodyUSDContruction(_citiFile), DataFormat.Json);
            }

            return request;
        }

        private string RequestBodyConstruction(CitiFileData data)
        {
            var body = @$"
:20:9400120212632012
:25:33387636
:28:186/1
:60F:C{data.Year}{data.Mounth}{data.Day}EUR61600,56
:62F:C{data.Year}{data.Mounth}{data.Day}EUR61736,87
:64:C{data.Year}{data.Mounth}{data.Day}EUR61736,87
-
:20:9400124594782059
:25:13041913
:28:186/1
:60F:C{data.Year}{data.Mounth}{data.Day}{data.Currency}103215,02
:61:{data.Year}{data.Mounth}{data.Day}{data.Mounth}{data.Day}CP{data.Amount}NTRF231048 04982601//95223524
/CTC/271/CREDIT TRANSFER RECD
:86:/PT/DE/EI/231048 04982601 {data.SenderName}/BI/{data.ExternalAccountId}/BO/04982601/BO1/{data.SenderName}
:62F:C{data.Year}{data.Mounth}{data.Day}{data.Currency}111080,75
:64:C{data.Year}{data.Mounth}{data.Day}{data.Currency}111080,75
-
:20:0212630312684051
:25:31268405
:28:263/1
:60F:C{data.Year}{data.Mounth}{data.Day}USD829753,58
:62F:C{data.Year}{data.Mounth}{data.Day}USD847813,46
:64:C{data.Year}{data.Mounth}{data.Day}USD847813,46
:65:C210921USD847813,46
-";
            return body;
        }

        private static string RequestBodyUSDContruction(CitiFileData data)
        {
            var body = $@"
:20:9400120212632012
:25:33387636
:28:186/1
:60F:C{data.Year}{data.Mounth}{data.Day}EUR61600,56
:62F:C{data.Year}{data.Mounth}{data.Day}EUR61736,87
:64:C{data.Year}{data.Mounth}{data.Day}EUR61736,87
-
:20:9400124594782059
:25:13041913
:28:186/1
:60F:C{data.Year}{data.Mounth}{data.Day}GBP103215,02
:62F:C{data.Year}{data.Mounth}{data.Day}GBP111080,75
:64:C{data.Year}{data.Mounth}{data.Day}GBP111080,75
-
:20:0212630312684051
:25:31268405
:28:263/1
:60F:C{data.Year}{data.Mounth}{data.Day}USD829753,58
:61:{data.Year}{data.Mounth}{data.Day}{data.Mounth}{data.Day}CD{data.Amount}NMSC37TFQK0ZVFYLA8K//22134000303
/CTC/294/ACH CREDIT
:86:/AB/031100209/BI/{data.ExternalAccountId}/BN/ModiX LLC/BO1/{data.SenderName}
/IREF/021000023234777/PC/PAYMENTS/ROC/37TFQK0ZVFYLA8K/PT/F
T
:62F:C{data.Year}{data.Mounth}{data.Day}USD847813,46
:64:C{data.Year}{data.Mounth}{data.Day}USD847813,46
:65:C210921USD847813,46
-";
            return body;
        }
    }
}
