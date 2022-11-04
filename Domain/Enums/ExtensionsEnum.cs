using Polly;
using System.ComponentModel;

namespace SellersFunding.PaymentOrchestration.Test.APITest.Domain.Enums
{
    public static class ExtensionsEnum
    {
        public static string ToDescriptionString<T>(this T val)
        {
            DescriptionAttribute[]? attributes = (DescriptionAttribute[])val
                .GetType()
                .GetField(val.ToString() ?? string.Empty)
                ?.GetCustomAttributes(typeof(DescriptionAttribute), false)!;
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
