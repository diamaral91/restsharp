using Polly;
using Polly.Retry;

namespace DailyAdvance.DigitalAccount.PO.ApiTests.Infrastructure.Database
{
    public static class WaitPolicy
    {
        private static readonly AsyncRetryPolicy _policy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5));

        public static AsyncRetryPolicy CheckQueryResult()
        {
            return Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5));
        }

        public static AsyncRetryPolicy CheckRequestResult()
        {
            return Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5));
        }

        public static async Task<IEnumerable<T>> RetryQuery<T>(this IEnumerable<T> query)
        {
            return await _policy.ExecuteAsync(async () =>
            {
                if (!query.Any())
                {
                    throw new Exception("query return null");
                }
                return query;
            });
        }
    }
}
