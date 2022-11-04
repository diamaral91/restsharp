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
    public class PayoutTransactionFullCollectionTest : TestBase
    {
        public PayoutTransactionFullCollectionTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact(DisplayName = "Create MarketPlace Payout and Approve Transaction with Full Collect")]
        public async Task UnmatchTransaction()
        {
            var balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            var balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\Balance");

            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
                _outputHelper.WriteLine("balance found: " + balance.ProductId);
            }

            var reprocessCitiFile = new DepositAccountApi(_citiFile).CreateCitiBankTransaction();

            var citiBankTransaction = reprocessCitiFile.Result.FirstOrDefault(t => _citiFile.CompanyId.Equals(t.CompanyId.ToString()));
            citiBankTransaction.ExternalAccountId.Should().Be(_citiFile.ExternalAccountId.ToString());

            var transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);
            var transactionId = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).TransactionId;
            _outputHelper.WriteLine("deposit account transactionId: " + transactionId.ToString());
            
            var amount = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).Amount;
            _outputHelper.WriteLine("transaction amount: " + amount.ToString());

            var approve = await _client.PostAsync(new DepositAccountApi(_citiFile).CreateApproveRequest(transactionId, EMarketplace.Amazon));
            approve.StatusCode.Should().Be(HttpStatusCode.OK);

            balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\BalanceMarketPlace");
            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
                _outputHelper.WriteLine("added balance found: " + balance.ProductId);
            }

            await _policy.ExecuteAsync(async () =>
            {
                var preTransactions = await new PaymentOrchestrationDB().SelectPreTransactionByAmount(amount, _citiFile.CompanyId);

                var digitalAccountDeposit = preTransactions.FirstOrDefault(pt => pt.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DigitalAccountDeposit)).BalanceId;
                digitalAccountDeposit.Should().Be(balances.FirstOrDefault(b => b.ProductId.Equals((int) EProductId.DigitalAccount)).Id);
                _outputHelper.WriteLine("select preTransaction by digital account deposit");

                var marketplacePayout = preTransactions.FirstOrDefault(pt => pt.ProductTransactionTypeId.Equals((int)EProductTransactionTypeId.MarketplacePayout)).BalanceId;
                marketplacePayout.Should().Be(balances.FirstOrDefault(b => b.ProductId.Equals((int)EProductId.Marketplace)).Id);
                _outputHelper.WriteLine("select preTransaction by marketplacePayout");
            });

            var dailyTransactionId = await _policy.ExecuteAsync(async () =>
            {
                var dailyPayment = await new RawDataDB().selectDailyPaymentTransactionByValue(amount);
                return dailyPayment.FirstOrDefault(t => amount.Equals(t.Value)).Id;
            });
            _outputHelper.WriteLine("select transaction on daily payment: " + dailyTransactionId);

            new CollectApi().Collect(dailyTransactionId, amount);
            _outputHelper.WriteLine("collected amount: " + amount);

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
                _outputHelper.WriteLine("transactions status: " + status[count]);
                count++;
            }

            var POTransactionId = _policy.ExecuteAsync(async () =>
            {
                var POTransaction = await new PaymentOrchestrationDB().SelectTransactionByAmount(_citiFile.CompanyId, amount);
                Assert.NotNull(POTransaction.FirstOrDefault(t => t.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DailyAdvancePrincipal)));
                _outputHelper.WriteLine("daily advance principal found on transaction PO");

                Assert.NotNull(POTransaction.FirstOrDefault(t => t.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DailyAdvanceCollect)));
                _outputHelper.WriteLine("daily advance collect found on transaction PO");
                return POTransaction.FirstOrDefault(t => t.Status.Equals((int)EPaymentOrchestrationStatus.Completed)).ExternalId;
            });
            Assert.NotNull(POTransactionId);

            var collect = await new CollectDB().selectCollect(amount, _citiFile.CompanyId);
            Assert.NotNull(collect.FirstOrDefault(c => amount.Equals(c.AmountCollected)));
            _outputHelper.WriteLine("amount " + amount.ToString() + " collect found on database");
        }
    }
}