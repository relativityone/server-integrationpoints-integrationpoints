using System;
using System.IO;
using Polly;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal interface IStreamRetryPolicyFactory
	{
		ISyncPolicy<Stream> Create(Action<int> onRetry, int retryCount, TimeSpan sleepDuration);
	}
}
