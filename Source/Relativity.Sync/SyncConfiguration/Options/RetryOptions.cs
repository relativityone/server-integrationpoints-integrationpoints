using Relativity.Services.Objects.DataContracts;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class RetryOptions
	{
		public RelativityObject JobToRetry { get; }

		public RetryOptions(RelativityObject jobToRetry)
		{
			JobToRetry = jobToRetry;
		}
	}
}
