using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Enums;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;
using DailyAdvance.DigitalAccount.PO.ApiTests.Functions.DepositAccount;
using DepositAccount.APITest.Infrastructure;
using DepositAccount.APITest.Infrastructure.database;
using FluentAssertions;
using RestSharp;
using System.Net;
using Xunit.Abstractions;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Test
{
    public class MatchTransactionTest : TestBase
    {
        private readonly DepositAccountDB _depositAccountDB = new DepositAccountDB();
        public MatchTransactionTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact(DisplayName = "Create Disbursement and Matched Transaction Approval")]
        public async Task MatchTransaction()
        {
            var efeg = await new AmazonFinancialEventGroupDB().GetFinancialEventGroup(_citiFile.CompanyId, _citiFile.Currency);
            var actualTransaction = efeg.FirstOrDefault(t => _citiFile.CompanyId.Equals(t.CompanyId.ToString()));
            var accountTail = actualTransaction.AccountTail.ToString();

            //_citiFile.Amount = actualTransaction.Amount.ToString();
            //var externalAccountId = _citiFile.ExternalAccountId.ToString();
            //_citiFile.ExternalAccountId = Int64.Parse(string.Concat(externalAccountId.Remove(externalAccountId.Length - 3), accountTail));
            //_citiFile.updateDate(actualTransaction.FundTransferDate);
            _citiFile.updateCitiFileDataWithAmazonFinancialEventGroupTransaction(actualTransaction);

            var account = await _depositAccountDB.GetAccount(_citiFile.CompanyId, _citiFile.Currency);
            account.Value = string.Concat(account.Value.Remove(account.Value.Length - 3), accountTail);
            account.ExternalAccountId = string.Concat(account.ExternalAccountId.Remove(account.ExternalAccountId.Length - 3), accountTail);
            account.ExtraData = string.Concat(account.ExtraData.Remove(account.ExtraData.Length - 5), accountTail, "\"}");

            _citiFile.ExtraData = account.ExtraData;
            _citiFile.Id = account.Id;

            await _depositAccountDB.UpdateAccount(_citiFile);
            await _depositAccountDB.UpdateAccountField(account.Value, account.Id.ToString());

            var reprocessCitiFile = new DepositAccountApi(_citiFile).CreateCitiBankTransaction();

            var actual = reprocessCitiFile.Result.FirstOrDefault(t => _citiFile.CompanyId.Equals(t.CompanyId.ToString()));
            actual.ExternalAccountId.Should().Be(_citiFile.ExternalAccountId);
            actual.Currency.Should().Be(_citiFile.Currency);
            actual.SenderName.Should().Be(_citiFile.SenderName);

            var transactions = await new DepositAccountDB().SelectTransaction(actual.ExternalTransactionId);

            await Task.Delay(10000);
            var transactionId = transactions.FirstOrDefault(t => int.Parse(_citiFile.CompanyId).Equals(t.CompanyId)).TransactionId;
            _outputHelper.WriteLine(transactionId.ToString());

            var approve = await _client.PostAsync(new DepositAccountApi(_citiFile).CreateApproveRequest(transactionId, EMarketplace.Amazon));
            approve.StatusCode.Should().Be(HttpStatusCode.OK);

            transactions = await new DepositAccountDB().SelectTransaction(actual.ExternalTransactionId);

            var status = new int[] { 1, 3, 11, 3, 5, 6 };
            var count = 0;
            foreach (var t in transactions)
            {
                t.TransactionId.Should().NotBeEmpty();
                //transactionId = t.TransactionId.ToString();

                t.CreatedDate.ToString().Contains(DateTime.UtcNow.ToString());
                t.CompanyId.Should().Be(int.Parse(_citiFile.CompanyId));
                t.Amount.Should().Be(double.Parse(_citiFile.Amount));
                t.Currency.Should().Be(_citiFile.Currency);
                t.ExternalAccountId.Should().Be(Int64.Parse(_citiFile.ExternalAccountId));
                t.StatusId.Should().Be(status[count]);
                count++;
            }
        }
    }
}