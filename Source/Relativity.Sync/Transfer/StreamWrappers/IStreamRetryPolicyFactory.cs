using System;
using System.IO;
using Polly;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal interface IStreamRetryPolicyFactory
    {
        IAsyncPolicy<Stream> Create(Func<Stream, bool> shouldRetry, Action<Stream, Exception, int> onRetry, int retryCount, TimeSpan sleepDuration);
    }
}
