using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database;

namespace DepositAccount.APITest.Infrastructure.database
{
    public class DepositAccountDB
    {
        private readonly ConnectionBase _connectBase;
        public DepositAccountDB()
        {
            _connectBase = new ConnectionBase();
        }

        public async Task<IEnumerable<TransactionDto>> SelectTransaction(string ExternalTransactionId)
        {
            return await WaitPolicy.CheckQueryResult().ExecuteAsync(async () =>
            {
                var dto = await _connectBase.Get<TransactionDto>($@"
                    SELECT t.TransactionId, 
                    t.CreatedDate, 
                    t.CompanyId,
                    t.Amount,
                    t.Currency,
                    t.ExternalAccountId,
                    t.MarketPlaceId,
                    t.StatusId from DepositAccount.[Transaction] t 
                    where t.ExternalTransactionId = @{nameof(ExternalTransactionId)}", new { ExternalTransactionId });
                if (!dto.Any())
                {
                    throw new Exception("not found transaction by externalTransactionId");
                }
                return dto;
            });
        }

        public async Task<IEnumerable<TransactionDto>> SelectTransactionByAmountAndNULLCompany(double amount)
        {
            return await WaitPolicy.CheckQueryResult().ExecuteAsync(async () =>
            {
                var dto = await _connectBase.Get<TransactionDto>($@"
                    SELECT t.TransactionId, 
                    t.CreatedDate, 
                    t.CompanyId,
                    t.Amount,
                    t.Currency,
                    t.ExternalAccountId,
                    t.StatusId from DepositAccount.[Transaction] t 
                    where t.Amount = @{nameof(amount)} 
                    and t.CompanyId IS NULL order by ID desc", new { amount });
                if (!dto.Any())
                {
                    throw new Exception("not found transaction by amount");
                }
                return dto;
            });
        }

        public async Task<AccountDto> GetAccount(string companyId, string currency)
        {
            return await WaitPolicy.CheckQueryResult().ExecuteAsync(async () =>
            {
                var dto = await _connectBase.GetFirst<AccountDto>($@"
                    SELECT da.ID as Id, da.ExternalAccountId, daf.Value, da.ExtraData 
                    FROM DepositAccount.Account da
                    INNER JOIN DepositAccount.AccountField daf 
                    ON daf.ID = da.ID
                    WHERE CompanyId = @{nameof(companyId)} AND  IsActive = '1'
                    AND Currency = @{nameof(currency)}", new { companyId, currency });
                if (dto.Equals(null))
                {
                    throw new Exception("not found account by companyId: " + companyId);
                }
                return dto;
            });
        }

        public async Task UpdateAccount(CitiFileData data)
        {
            await _connectBase.UpdateDepositAccount($@"
                UPDATE
                  DepositAccount.Account
                SET
                  ExternalAccountId = @{nameof(data.ExternalAccountId)},
                  ExtraData = @{nameof(data.ExtraData)}
                WHERE CompanyId = @{nameof(data.CompanyId)} AND  IsActive = '1'
                AND Currency = @{nameof(data.Currency)} AND ID = @{nameof(data.Id)}", 
                new { data.ExternalAccountId, data.ExtraData, data.CompanyId, data.Currency, data.Id });
        }

        public async Task UpdateAccountField(string value, string accountId)
        {
            await _connectBase.UpdateDepositAccount($@"
                UPDATE
                  DepositAccount.AccountField
                SET
                  Value = @{nameof(value)}
                WHERE ID = @{nameof(accountId)}", new { value, accountId });
        }
    }
}
