using System;
using Atata;
using Polly;
using Polly.Retry;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Extensions
{
    internal static class UIComponentsExtension
    {
        internal static TOwner BeVisibleWithRetries<TComponent, TOwner>(
            this IUIComponentVerificationProvider<TComponent, TOwner> should,
            int maxRetriesCount = 3,
            double betweenRetriesBase = 2,
            int maxJitterMs = 100)
            where TComponent : UIComponent<TOwner>
            where TOwner : PageObject<TOwner>
        {
            RetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    maxRetriesCount,
                    retryAttempt =>
            {
                TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(betweenRetriesBase, retryAttempt));
                TimeSpan jitter = TimeSpan.FromMilliseconds(new Random().Next(0, maxJitterMs));
                return delay + jitter;
            });

            TOwner isVisible = retryPolicy.Execute(should.BeVisible);
            return isVisible;
        }
    }
}
