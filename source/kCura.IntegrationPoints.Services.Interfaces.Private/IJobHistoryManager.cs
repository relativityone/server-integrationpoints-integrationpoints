using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Interfaces.Private.Requests;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	/// Enables access to information regarding integration points
	/// </summary>
	[WebService("Job History Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IJobHistoryManager: IDisposable
	{
		/// <summary>
		/// Pings the service to ensure its up and running
		/// </summary>
		Task<bool> PingAsync();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Task<JobHistorySummaryModel> GetJobHistory(JobHistoryRequest request);
	}
}