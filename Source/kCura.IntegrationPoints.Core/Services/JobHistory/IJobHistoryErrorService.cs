using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IJobHistoryErrorService
    {
        Data.JobHistory JobHistory { get; set; }
        IntegrationPointDto IntegrationPointDto { get; set; }
        IJobStopManager JobStopManager { get; set; }

        void SubscribeToBatchReporterEvents(object batchReporter);
        void CommitErrors();
        void AddError(ChoiceRef errorType, Exception ex);
        void AddError(ChoiceRef errorType, string documentIdentifier, string errorMessage, string stackTrace);
    }
}
