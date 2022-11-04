using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model
{
    public class CitiFileData
    {
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public string Year { get; set; }
        public string Mounth { get; set; }
        public string Day { get; set; }
        public string Amount { get; set; }
        public string ExternalAccountId { get; set; }
        public string Currency { get; set; }
        public string SenderName { get; set; }
        public string ExtraData { get; set; }
        public double LeftOverFromCollection { get; set; }

        public CitiFileData(string fixture)
        {
            var matchFixture = DataHelper.JsonToDto<MatchDto>(fixture);
            CompanyId = matchFixture.CompanyId;

            var date = DateTime.Now;
            var formatDate = String.Format("{0:yy/MM/dd}", date);

            Year = formatDate.Split("/")[0];
            Mounth = formatDate.Split("/")[1];
            Day = formatDate.Split("/")[2];

            Random r = new Random();
            var value = r.Next(0, 300);
            Amount = String.Format("{0:0.00}", value);

            ExternalAccountId = matchFixture.ExternalAccountId;
            Currency = matchFixture.Currency;
            SenderName = matchFixture.SenderName;

            LeftOverFromCollection = double.Parse(matchFixture.LeftOverFromCollection);
        }

        public void updateDate(DateTime date)
        {
            var formatDate = String.Format("{0:yy/MM/dd}", date);

            Year = formatDate.Split("/")[0];
            Mounth = formatDate.Split("/")[1];
            Day = formatDate.Split("/")[2];
        }

        public void updateCitiFileDataWithAmazonFinancialEventGroupTransaction(FinancialEventGroupDto feg)
        {
            Amount = feg.Amount.ToString();
            var externalAccountId = ExternalAccountId.ToString();
            ExternalAccountId = string.Concat(externalAccountId.Remove(externalAccountId.Length - 3), feg.AccountTail.ToString());

            var formatDate = String.Format("{0:yy/MM/dd}", feg.FundTransferDate);

            Year = formatDate.Split("/")[0];
            Mounth = formatDate.Split("/")[1];
            Day = formatDate.Split("/")[2];


        }
    }
}
