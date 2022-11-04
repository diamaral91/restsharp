using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto.PaymentOrchestration;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Enums;
using DailyAdvance.DigitalAccount.PO.ApiTests.Functions;
using DailyAdvance.DigitalAccount.PO.ApiTests.Functions.BankTransfers;
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
    public class PayoutTransactionParcialCollectionTest : TestBase
    {
        public PayoutTransactionParcialCollectionTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact(DisplayName = "Create UnMatched MarketPlace Payout and Approve Transaction with Parcial Collect")]
        public async Task UnmatchTransaction()
        {
            var balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            var balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\Balance");

            // validar que o seller já possui os balances [externalAccount, dailyAdvance, digitalAccount]
            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
            }

            var reprocessCitiFile = new DepositAccountApi(_citiFile).CreateCitiBankTransaction();

            var citiBankTransaction = reprocessCitiFile.Result.FirstOrDefault(t => _citiFile.CompanyId.Equals(t.CompanyId.ToString()));
            citiBankTransaction.ExternalAccountId.Should().Be(_citiFile.ExternalAccountId);

            var transactions = await new DepositAccountDB().SelectTransaction(citiBankTransaction.ExternalTransactionId);
            var transactionId = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).TransactionId;
            _outputHelper.WriteLine(transactionId.ToString());

            var approve = await _client.PostAsync(new DepositAccountApi(_citiFile).CreateApproveRequest(transactionId, EMarketplace.Amazon));
            approve.StatusCode.Should().Be(HttpStatusCode.OK);

            // validar a criação do balance marketPlace
            balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\BalanceMarketPlace");
            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
            }

            var amount = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).Amount;
            _outputHelper.WriteLine(amount.ToString());

            var digitalAccountDeposit = await _policy.ExecuteAsync(async () =>
            {
                var preTransactions = await new PaymentOrchestrationDB().SelectPreTransactionByAmount(amount, _citiFile.CompanyId);
                return preTransactions.FirstOrDefault(pt => pt.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DigitalAccountDeposit)).BalanceId;
            });

            var marketplacePayout = await _policy.ExecuteAsync(async () =>
            {
                var preTransactions = await new PaymentOrchestrationDB().SelectPreTransactionByAmount(amount, _citiFile.CompanyId);
                return preTransactions.FirstOrDefault(pt => pt.ProductTransactionTypeId.Equals((int)EProductTransactionTypeId.MarketplacePayout)).BalanceId;
            });

            digitalAccountDeposit.Should().Be(balances.FirstOrDefault(b => b.ProductId.Equals((int) EProductId.DigitalAccount)).Id);
            marketplacePayout.Should().Be(balances.FirstOrDefault(b => b.ProductId.Equals((int) EProductId.Marketplace)).Id);

            var dailyTransactionId = await _policy.ExecuteAsync(async () =>
            {
                var dailyPayment = await new RawDataDB().selectDailyPaymentTransactionByValue(amount);
                return dailyPayment.FirstOrDefault(t => amount.Equals(t.Value)).Id;
            });

            var leftOver = _citiFile.LeftOverFromCollection;
            new CollectApi().Collect(dailyTransactionId, amount - leftOver);

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

            var POTransactionId = await _policy.ExecuteAsync(async () =>
            {
                var POTransaction = await new PaymentOrchestrationDB().SelectTransactionByAmount(_citiFile.CompanyId, leftOver);
                return POTransaction.FirstOrDefault(t => t.Status.Equals((int)EPaymentOrchestrationStatus.Pending)).ExternalId;
            });
            Assert.NotNull(POTransactionId);

            var DailyAdvancePrincipal = await _policy.ExecuteAsync(async () =>
            {
                var POTransaction = await new PaymentOrchestrationDB().SelectTransactionByAmount(_citiFile.CompanyId, amount - leftOver);
                return POTransaction.FirstOrDefault(t => t.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DailyAdvancePrincipal)).ProductTransactionTypeId;
            });

            var DailyAdvanceCollect = await _policy.ExecuteAsync(async () =>
            {
                var POTransaction = await new PaymentOrchestrationDB().SelectTransactionByAmount(_citiFile.CompanyId, amount - leftOver);
                return POTransaction.FirstOrDefault(t => t.ProductTransactionTypeId.Equals((int) EProductTransactionTypeId.DailyAdvanceCollect)).ProductTransactionTypeId;
            });


            Assert.NotNull(DailyAdvancePrincipal);
            Assert.NotNull(DailyAdvanceCollect);

            var bankTransfer = await new BankTransferDB().selectTransfersById(POTransactionId);

            new BankTransferApi().PostFakeBank(bankTransfer.TransferId, 5);

            await Task.Delay(5000);
            new BankTransferApi().PostFakeBank(bankTransfer.TransferId, 6);

            bankTransfer.Amount.Should().Be(leftOver);
            bankTransfer.SourceRequest.Should().Be("payout-preferences");
        }
    }
}