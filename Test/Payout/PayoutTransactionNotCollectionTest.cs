using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto.PaymentOrchestration;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Enums;
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
    public class PayoutTransactionNotCollectionTest : TestBase
    {
        public PayoutTransactionNotCollectionTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact(DisplayName = "Create MarketPlace Payout and Approve Transaction without Collect")]
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

            var actual = reprocessCitiFile.Result.FirstOrDefault(t => _citiFile.CompanyId.Equals(t.CompanyId.ToString()));
            actual.ExternalAccountId.Should().Be(_citiFile.ExternalAccountId);
            actual.Currency.Should().Be(_citiFile.Currency);
            actual.SenderName.Should().Be(_citiFile.SenderName);

            // validar a criação do balance marketPlace
            await Task.Delay(15000);
            var transactions = await new DepositAccountDB().SelectTransaction(actual.ExternalTransactionId);
            var transactionId = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).TransactionId;
            _outputHelper.WriteLine(transactionId.ToString());

            var approve = await _client.PostAsync(new DepositAccountApi(_citiFile).CreateApproveRequest(transactionId, EMarketplace.Amazon));
            approve.StatusCode.Should().Be(HttpStatusCode.OK);

            balances = await new PaymentOrchestrationDB().SelectBalanceByCompanyId(_citiFile.CompanyId);
            balancesFixture = DataHelper.JsonToList<BalanceDto>("PaymentOrchestration\\BalanceMarketPlace");
            foreach (var balance in balancesFixture)
            {
                Assert.NotNull(balances.FirstOrDefault(b => balance.ProductId.Equals(b.ProductId)));
            }

            var amount = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).Amount;

            var preTransactions = await new PaymentOrchestrationDB().SelectPreTransactionByAmount(amount, _citiFile.CompanyId);
            var preTransactionsFixture = DataHelper.JsonToList<PreTransactionDto>("PaymentOrchestration\\PreTransaction");

            // validar que o seller já possui os balances [externalAccount, dailyAdvance, digitalAccount]
            foreach (var preTransaction in preTransactionsFixture)
            {
                var pt = preTransactions.FirstOrDefault(pt => preTransaction.ProductTransactionTypeId.Equals(pt.ProductTransactionTypeId));
                pt.CompanyId.Should().Be(int.Parse(_citiFile.CompanyId));
            }

            _outputHelper.WriteLine(amount.ToString());
            transactions = await new DepositAccountDB().SelectTransaction(actual.ExternalTransactionId);

            var status = new int[] { 1, 3, 5, 6 };
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

            var POTransaction = await new PaymentOrchestrationDB().SelectTransactionByAmount(_citiFile.CompanyId, amount);
            var transactionPO = POTransaction.FirstOrDefault(t => t.Status.Equals(6));

            var bankTransfer = await new BankTransferDB().selectTransfersById(transactionPO.ExternalId);

            new BankTransferApi().PostFakeBank(bankTransfer.TransferId, 5);

            await Task.Delay(5000);
            new BankTransferApi().PostFakeBank(bankTransfer.TransferId, 6);

            bankTransfer.Amount.Should().Be(amount);
            bankTransfer.SourceRequest.Should().Be("payout-preferences");
        }
    }
}