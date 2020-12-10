using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class RetryOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public RelativityObject JobToRetry { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="jobToRetry"></param>
		public RetryOptions(RelativityObject jobToRetry)
		{
			JobToRetry = jobToRetry;
		}
	}
}
