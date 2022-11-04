namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto.PaymentOrchestration
{
    public class PreTransactionDto
    {
        public int CompanyId { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public int BalanceId { get; set; }
        public int ProductTransactionTypeId { get; set; }
    }
}
