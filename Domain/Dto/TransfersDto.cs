namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    public class TransfersDto
    {
        public Guid TransferId { get; set; }
        public string Reason { get; set; }
        public string SourceRequest { get; set; }
        public double Amount { get; set; }
        public string ExtraData { get; set; }
    }
}
