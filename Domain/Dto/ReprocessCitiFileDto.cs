using Newtonsoft.Json;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    public class ReprocessCitiFileDto
    {
        public Guid TransactionId { get; set; }
        public int CompanyId { get; set; }
        public string ExternalAccountId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string Currency { get; set; }
        public string SenderName { get; set; }
    }
}
