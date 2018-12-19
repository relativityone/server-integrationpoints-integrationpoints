using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal sealed class SyncJob : ISyncJob
	{
		public Task ExecuteAsync(CancellationToken token)
		{
			throw new NotImplementedException();
		}

		public Task RetryAsync(CancellationToken token)
		{
			throw new NotImplementedException();
		}
	}
}