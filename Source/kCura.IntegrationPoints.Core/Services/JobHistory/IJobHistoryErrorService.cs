using System;
using kCura.IntegrationPoints.Core.Managers;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public interface IJobHistoryErrorService
	{
		Data.JobHistory JobHistory { get; set; }
		Data.IntegrationPoint IntegrationPoint { get; set; }
		IJobStopManager JobStopManager { get; set; }
		bool JobLevelErrorOccurred { get; }

		void SubscribeToBatchReporterEvents(object batchReporter);
		void CommitErrors();
		void AddError(Relativity.Client.DTOs.Choice errorType, Exception ex);
		void AddError(Relativity.Client.DTOs.Choice errorType, string documentIdentifier, string errorMessage, string stackTrace);
	}
}