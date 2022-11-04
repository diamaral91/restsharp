using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto.PaymentOrchestration;
using DepositAccount.APITest.Infrastructure.database;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database
{
    public class PaymentOrchestrationDB
    {

        private readonly ConnectionBase _connectBase;

        public PaymentOrchestrationDB()
        {
            _connectBase = new ConnectionBase("ConnectionString_PaymentOrchestration");
        }

        public async Task<IEnumerable<POTransactionDto>> SelectTransactionByAmount(string companyId, double amount)
        {
            return await _connectBase.Get<POTransactionDto>($@"
                SELECT * FROM [Transaction] 
                WHERE CompanyId = @{nameof(companyId)} AND Amount = @{nameof(amount)} 
                ORDER BY CreateAt DESC", new { companyId, amount });
        }

        public async Task<IEnumerable<BalanceDto>> SelectBalanceByCompanyId(string companyId)
        {
            return await _connectBase.Get<BalanceDto>($@"
                SELECT CompanyId, Currency, ProductId, Id 
                FROM Balance 
                WHERE CompanyId = @{nameof(companyId)}", new { companyId });
        }

        public async Task<IEnumerable<PreTransactionDto>> SelectPreTransactionByAmount(double amount, string companyId)
        {
            return await _connectBase.Get<PreTransactionDto>($@"
                select * from PreTransaction 
                where Amount = @{nameof(amount)} and CompanyId = @{nameof(companyId)} 
                order by CreateAt desc", new { amount, companyId });
        }
    }
}
