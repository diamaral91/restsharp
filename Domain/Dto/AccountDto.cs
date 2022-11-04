using Newtonsoft.Json;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Domain.Dto
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AccountDto
    {
        public int Id { get; set; }
        public string ExternalAccountId{ get; set; }
        public string Value { get; set; }
        public string ExtraData { get; set; }
    }
}
