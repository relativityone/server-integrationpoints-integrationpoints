using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class RetryOptions
	{
		public RelativityObject JobToRetry { get; set; }

		public RetryOptions(RelativityObject jobToRetry)
		{
			JobToRetry = jobToRetry;
		}
	}
}
