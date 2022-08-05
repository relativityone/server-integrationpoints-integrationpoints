using System.Net.Http;
using Polly.Retry;

namespace Relativity.Sync.HttpClient
{
    public interface IHttpClientRetryPolicyProvider
    {
        RetryPolicy<HttpResponseMessage> GetPolicy(int maxRetryCount = 5);
    }
}
