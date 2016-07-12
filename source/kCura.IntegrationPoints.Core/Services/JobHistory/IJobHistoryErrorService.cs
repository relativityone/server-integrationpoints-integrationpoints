using System;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public interface IJobHistoryErrorService
    {
        void CommitErrors();
        void AddError(Relativity.Client.Choice errorType, Exception ex);
        void AddError(Relativity.Client.Choice errorType, string documentIdentifier, string errorMessage, string stackTrace);
    }
}