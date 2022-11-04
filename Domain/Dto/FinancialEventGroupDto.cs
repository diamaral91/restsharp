using Newtonsoft.Json;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FinancialEventGroupDto
    {
        public string Currency { get; set; }
        public double Amount { get; set; }
        public DateTime FundTransferDate { get; set; }
        public int CompanyId { get; set; }
        public int AccountTail { get; set; }
    }
}
