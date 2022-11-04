using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _outputHelper;
        public UnitTest1(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact(DisplayName = "Wait Query")]
        public void Poc()
        {
            Console.WriteLine("NUNIT");
            _outputHelper.WriteLine("UNIT TEST 1");
        }
    }
}