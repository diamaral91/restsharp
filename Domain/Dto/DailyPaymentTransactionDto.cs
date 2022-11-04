namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    public class DailyPaymentTransactionDto
    {
        public int Id { get; set; }
        public int DailyBalanceId  { get; set; }
        public double Value { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public int TypeId { get; set; }
    }
}