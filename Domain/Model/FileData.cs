using DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto;
using DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Model
{
    public class FileData
    {
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public string Year { get; set; }
        public string Mounth { get; set; }
        public string Day { get; set; }
        public string Amount { get; set; }
        public Int64 ExternalAccountId { get; set; }
        public string Currency { get; set; }
        public string SenderName { get; set; }
        public string ExtraData { get; set; }

        public FileData(string fixture)
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

            ExternalAccountId = Int64.Parse(matchFixture.ExternalAccountId);
            Currency = matchFixture.Currency;
            SenderName = matchFixture.SenderName;
        }

        public void updateDate(DateTime date)
        {
            var formatDate = String.Format("{0:yy/MM/dd}", date);

            Year = formatDate.Split("/")[0];
            Mounth = formatDate.Split("/")[1];
            Day = formatDate.Split("/")[2];
        }
    }
}
