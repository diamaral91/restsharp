using Newtonsoft.Json;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure
{
    public class DataHelper
    {
        public static List<T> JsonToList<T>(string fileName)
        {
            var file = new StreamReader($"Domain\\Fixtures\\{fileName}.json");
            var jsonString = file.ReadToEnd();
            return JsonConvert.DeserializeObject<List<T>>(jsonString) ?? new List<T>();
        }

        public static string JsonToString<T>(string fileName)
        {
            var file = new StreamReader($"Domain\\Fixtures\\{fileName}.json");
            var jsonString = file.ReadToEnd();
            return JsonConvert.DeserializeObject<string>(jsonString);
        }

        public static T JsonToDto<T>(string fileName)
        {
            var file = new StreamReader($"Domain\\Fixtures\\{fileName}.json");
            var jsonString = file.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}