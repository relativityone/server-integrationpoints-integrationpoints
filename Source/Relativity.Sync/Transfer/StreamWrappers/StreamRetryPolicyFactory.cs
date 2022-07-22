using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Polly;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    [ExcludeFromCodeCoverage] // Just invokes third-party static method. More extensively tested in integration tests.
    internal sealed class StreamRetryPolicyFactory : IStreamRetryPolicyFactory
    {
        public IAsyncPolicy<Stream> Create(Func<Stream, bool> shouldRetry, Action<Stream, Exception, int> onRetry, int retryCount, TimeSpan sleepDuration)
        {
            return Policy
                .HandleResult(shouldRetry)
                .Or<Exception>()
                .WaitAndRetryAsync(retryCount, i => sleepDuration, (result, dur, i, ctx) => onRetry(result.Result, result.Exception, i));
        }
    }
}
