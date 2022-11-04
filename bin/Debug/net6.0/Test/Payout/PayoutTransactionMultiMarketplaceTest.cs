using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto.PaymentOrchestration;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Enums;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;
using DailyAdvance.DigitalAccount.PO.ApiTests.Functions.DepositAccount;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database;
using DepositAccount.APITest.Infrastructure;
using DepositAccount.APITest.Infrastructure.database;
using FluentAssertions;
using Polly;
using RestSharp;
using SellersFunding.PaymentOrchestration.Test.APITest.Domain.Enums;
using System.Net;
using Xunit.Abstractions;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Payout
{
    public class PayoutTransactionMultiMarketplaceTest : TestBase
    {
        public PayoutTransactionMultiMarketplaceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact(DisplayName = "Create Multiple MarketPlace Payout and Approve Transaction with Differents Marketplace")]
        public async Task UnmatchTransaction()
        {
            var citiFile = new CitiFileData("PaymentData");

            var balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(citiFile.CompanyId);
            var balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\Balance");

            // validar que o seller já possui os balances [externalAccount, dailyAdvance, digitalAccount]
            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
            }

            EMarketplace[] marketplace = new[] { EMarketplace.Amazon, EMarketplace.Walmart, EMarketplace.Amazon };
            foreach (var mkt in marketplace)
            {
                var reprocessCitiFile = new DepositAccountApi(citiFile).CreateCitiBankTransaction();
                _outputHelper.WriteLine(citiFile.Amount);

                var citiBankTransaction = reprocessCitiFile.Result.FirstOrDefault(t => citiFile.CompanyId.Equals(t.CompanyId.ToString()));
                citiBankTransaction.ExternalAccountId.Should().Be(citiFile.ExternalAccountId.ToString());

                var transactionId = await _policy.ExecuteAsync(async () =>
                {
                    var transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);
                    return transactions.FirstOrDefault(t => int.Parse(citiFile.CompanyId).Equals(t.CompanyId)).TransactionId;
                });

                _outputHelper.WriteLine(transactionId.ToString());

                await Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5)).ExecuteAsync(async ()=> {
                    var approve = await _client.PostAsync(new DepositAccountApi(citiFile).CreateApproveRequest(transactionId, mkt));
                    approve.StatusCode.Should().Be(HttpStatusCode.OK);
                });

                balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(citiFile.CompanyId);
                balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\BalanceMarketPlace");
                foreach (var balance in balancesFixture)
                {
                    Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
                }

                await _policy.ExecuteAsync(async () =>
                {
                    var transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);
                    return transactions.FirstOrDefault(t => t.StatusId.Equals((int) EDepositAccountStatus.DisbursementSent)).MarketPlaceId.Should().Be(mkt.ToDescriptionString().ToUpper());
                });

                citiFile = new CitiFileData("PaymentData");
            }

            balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(citiFile.CompanyId);
            EProductId[] productIds = new[] { EProductId.ExternalAccount, EProductId.DigitalAccount, EProductId.DailyAdvance, EProductId.Marketplace, EProductId.Marketplace };

            for (int b = 0; b < balances.Count(); b++)
            {
                balances.ElementAt(b).ProductId.Should().Be((int) productIds[b]);
            }
        }
    }
}