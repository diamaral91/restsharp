namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    public class CollectDto
    {
        public int Id { get; set; }
        public Guid ExternalId { get; set; }
        public int ProductId { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public int CompanyId { get; set; }
        public int Collected { get; set; }
        public double AmountCollected { get; set; }
    }
}
