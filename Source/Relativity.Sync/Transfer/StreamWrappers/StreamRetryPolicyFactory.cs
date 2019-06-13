using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Polly;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	[ExcludeFromCodeCoverage] // Just invokes third-party static method. More extensively tested in integration tests.
	internal sealed class StreamRetryPolicyFactory : IStreamRetryPolicyFactory
	{
		public ISyncPolicy<Stream> Create(Action<int> onRetry, int retryCount, TimeSpan sleepDuration)
		{
			return Policy
				.HandleResult<Stream>(s => s == null || !s.CanRead)
				.Or<Exception>()
				.WaitAndRetry(retryCount, i => sleepDuration, (result, dur, i, ctx) => onRetry(i));
		}
	}
}
