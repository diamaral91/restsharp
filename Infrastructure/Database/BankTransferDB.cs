using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DepositAccount.APITest.Infrastructure.database;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database
{
    public class BankTransferDB
    {
        private readonly ConnectionBase _connectBase;
        public BankTransferDB()
        {
            _connectBase = new ConnectionBase("ConnectionString_Transfers");
        }

        public async Task<TransfersDto> selectTransfersById(Guid transferId)
        {
            return await _connectBase.GetFirst<TransfersDto>($@"
                SELECT * FROM Transfers where TransferId = @{nameof(transferId)}", new {transferId});
        }
    }
}
