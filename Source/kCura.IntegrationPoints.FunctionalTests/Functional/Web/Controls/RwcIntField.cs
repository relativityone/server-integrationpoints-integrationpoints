using System;
using Atata;
using Polly;
using Polly.Retry;
using Relativity.Testing.Framework.Web.Components;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Controls
{
    public class RwcIntField<TOwner> : RwcTextField<string, TOwner> where TOwner : PageObject<TOwner>
    {
        public int GetValueWithRetries(int maxRetriesCount = 3, double betweenRetriesBase = 2, int maxJitterMs = 100)
        {
            RetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    maxRetriesCount,
                    retryAttempt =>
            {
                TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(betweenRetriesBase, retryAttempt));
                TimeSpan jitter = TimeSpan.FromMilliseconds(new Random().Next(0, maxJitterMs));
                return delay + jitter;
            });

            int parsedValue = retryPolicy.Execute(() =>
            {
                int value = int.Parse(Value);
                return value;
            });

            return parsedValue;
        }
    }
}
