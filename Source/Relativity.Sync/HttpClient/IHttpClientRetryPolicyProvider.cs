using System.Net.Http;
using Polly.Retry;

namespace Relativity.Sync.HttpClient
{
    internal interface IHttpClientRetryPolicyProvider
    {
        RetryPolicy<HttpResponseMessage> GetPolicy(int maxRetryCount = 5);
    }
}
