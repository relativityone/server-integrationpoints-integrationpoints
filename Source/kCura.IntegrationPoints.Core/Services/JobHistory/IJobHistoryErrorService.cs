using System;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IJobHistoryErrorService
    {
		Data.JobHistory JobHistory { get; set; }
		IntegrationPoint IntegrationPoint { get; set; }
		IJobStopManager JobStopManager { get; set; }
		bool JobLevelErrorOccurred { get; }

		void SubscribeToBatchReporterEvents(object batchReporter);
		void CommitErrors();
        void AddError(Relativity.Client.DTOs.Choice errorType, Exception ex);
        void AddError(Relativity.Client.DTOs.Choice errorType, string documentIdentifier, string errorMessage, string stackTrace);
    }
}