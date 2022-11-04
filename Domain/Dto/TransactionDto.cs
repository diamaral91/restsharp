using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyId { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public Int64 ExternalAccountId { get; set; }
        public int StatusId { get; set; }
        public string MarketPlaceId { get; set; }
    }
}
