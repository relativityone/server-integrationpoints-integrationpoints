using Relativity.Services.Objects.DataContracts;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class RetryOptions
	{
		public int JobToRetry { get; }

		public RetryOptions(int jobToRetry)
		{
			JobToRetry = jobToRetry;
		}
	}
}
