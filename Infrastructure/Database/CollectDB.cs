using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DepositAccount.APITest.Infrastructure.database;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database
{
    public class CollectDB
    {
        private readonly ConnectionBase _connectBase;
        public CollectDB()
        {
            _connectBase = new ConnectionBase("ConnectionString_Collect");
        }

        public async Task<IEnumerable<CollectDto>> selectCollect(double amount, string companyId)
        {
            await Task.Delay(5000);
            return await _connectBase.Get<CollectDto>($@"
                select * from Collect where Amount = @{nameof(amount)} and CompanyId = @{nameof(companyId)}", new {amount, companyId});
        }
    }
}
