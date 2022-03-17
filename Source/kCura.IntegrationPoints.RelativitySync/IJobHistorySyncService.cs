using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface IJobHistorySyncService
	{
		Task<RelativityObject> GetLastJobHistoryWithErrorsAsync(int workspaceID, int integrationPointArtifactID);

		Task UpdateJobStatusAsync(string syncStatus, IExtendedJob job);

		Task MarkJobAsValidationFailedAsync(IExtendedJob job, Exception ex);

		Task MarkJobAsStoppedAsync(IExtendedJob job);

		Task MarkJobAsSuspendingAsync(IExtendedJob job);

		Task MarkJobAsSuspendedAsync(IExtendedJob job);

		Task MarkJobAsFailedAsync(IExtendedJob job, Exception e);

		Task MarkJobAsStartedAsync(IExtendedJob job);

		Task MarkJobAsCompletedAsync(IExtendedJob job);
	}
}
