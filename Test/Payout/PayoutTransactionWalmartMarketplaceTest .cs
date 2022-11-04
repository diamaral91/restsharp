using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto.PaymentOrchestration;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Enums;
using DailyAdvance.DigitalAccount.PO.ApiTests.Functions;
using DailyAdvance.DigitalAccount.PO.ApiTests.Functions.DepositAccount;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database;
using DepositAccount.APITest.Infrastructure;
using DepositAccount.APITest.Infrastructure.database;
using FluentAssertions;
using RestSharp;
using System.Net;
using Xunit.Abstractions;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Payout
{
    public class PayoutTransactionWalmartMarketplaceTest : TestBase
    {
        public PayoutTransactionWalmartMarketplaceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact(DisplayName = "Create MarketPlace Payout and Approve Transaction with Full Collect To Walmart MarketPlace")]
        public async Task WalmartMarketplace()
        {
            var balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            var balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\Balance");

            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
            }

            var reprocessCitiFile = new DepositAccountApi(_citiFile).CreateCitiBankTransaction();

            var citiBankTransaction = reprocessCitiFile.Result.FirstOrDefault(t => _citiFile.CompanyId.Equals(t.CompanyId.ToString()));
            citiBankTransaction.ExternalAccountId.Should().Be(_citiFile.ExternalAccountId.ToString());

            var transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);
            var transactionId = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).TransactionId;
            _outputHelper.WriteLine(transactionId.ToString());

            var approve = await _client.PostAsync(new DepositAccountApi(_citiFile).CreateApproveRequest(transactionId, EMarketplace.Walmart));
            approve.StatusCode.Should().Be(HttpStatusCode.OK);

            await _policy.ExecuteAsync(async () =>
            {
                transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);
                Assert.NotNull(transactions.First(t => t.StatusId.Equals((int)EDepositAccountStatus.DisbursementSuccess)));
            });

            balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\BalanceMarketPlace");
            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
            }

            var amount = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).Amount;
            _outputHelper.WriteLine(amount.ToString());

            var preTransactions = await new PaymentOrchestrationDB().SelectPreTransactionByAmount(amount, _citiFile.CompanyId);
            var ptti = new[] {
                EProductTransactionTypeId.MoneyReceivedExternalAccount,
                EProductTransactionTypeId.DigitalAccountRelease,
                EProductTransactionTypeId.DigitalAccountDeposit,
                EProductTransactionTypeId.MarketplacePayout
                 };
            for(int countPreTransaction = 0; countPreTransaction < preTransactions.Count(); countPreTransaction++)
            {
                preTransactions.ElementAt(countPreTransaction).ProductTransactionTypeId.Should().Be((int) ptti[countPreTransaction]);
                var balanceId = preTransactions.ElementAt(countPreTransaction).BalanceId;
                balanceId.Should().Be(preTransactions.FirstOrDefault(pt => pt.ProductTransactionTypeId.Equals((int)ptti[countPreTransaction])).BalanceId);
            }

            var dailyPayment = await new RawDataDB().selectDailyPaymentTransactionByValue(amount);
            var dailyTransactionId = dailyPayment.FirstOrDefault(t => amount.Equals(t.Value)).Id;

            new CollectApi().Collect(dailyTransactionId, amount);

            transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);

            var status = Enum.GetValues(typeof(EDepositAccountStatus)).Cast<int>().ToList();
            var count = 0;
            foreach (var transaction in transactions)
            {
                transaction.TransactionId.Should().NotBeEmpty();
                transaction.CreatedDate.ToString().Contains(DateTime.UtcNow.ToString());
                transaction.CompanyId.Should().Be(int.Parse(_citiFile.CompanyId));
                transaction.Amount.Should().Be(double.Parse(_citiFile.Amount));
                transaction.Currency.Should().Be(_citiFile.Currency);
                transaction.ExternalAccountId.Should().Be(Int64.Parse(_citiFile.ExternalAccountId));
                transaction.StatusId.Should().Be(status[count]);
                count++;
            }

            var POTransactionId = _policy.ExecuteAsync(async () =>
            {
                var POTransaction = await new PaymentOrchestrationDB().SelectTransactionByAmount(_citiFile.CompanyId, amount);
                Assert.NotNull(POTransaction.FirstOrDefault(t => t.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DailyAdvancePrincipal)));
                Assert.NotNull(POTransaction.FirstOrDefault(t => t.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DailyAdvanceCollect)));
                return POTransaction.FirstOrDefault(t => t.Status.Equals((int)EPaymentOrchestrationStatus.Completed)).ExternalId;
            });
            Assert.NotNull(POTransactionId);

            var collect = await new CollectDB().selectCollect(amount, _citiFile.CompanyId);
            Assert.NotNull(collect.FirstOrDefault(c => amount.Equals(c.AmountCollected)));
        }
    }
}