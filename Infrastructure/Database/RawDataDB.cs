using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DepositAccount.APITest.Infrastructure.database;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database
{
    public class RawDataDB
    {
        private readonly ConnectionBase _connectBase;
       
        public RawDataDB()
        {
            _connectBase = new ConnectionBase("ConnectionString_RawData");
        }

        public async Task<IEnumerable<DailyPaymentTransactionDto>> selectDailyPaymentTransactionByValue(double value)
        {
            return await WaitPolicy.CheckQueryResult().ExecuteAsync(async () =>
            {
                var dto = await _connectBase.Get<DailyPaymentTransactionDto>($@"
                    SELECT * FROM DailyPayment.[Transaction] WHERE Value = @{nameof(value)}", new { value });
                if (!dto.Any())
                {
                    throw new Exception("not found daily transaction by value: " + value);
                }
                return dto;
            });
        }
    }
}