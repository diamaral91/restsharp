namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    public class POTransactionDto
    {
        public Guid ExternalId { get; set; }
        public double Amount { get; set; }
        public int CompanyId { get; set; }
        public int Status { get; set; }
        public string Currency { get; set; }
        public DateTime CreateAt { get; set; }
        public int ProductTransactionTypeId { get; set; }
    }
}
