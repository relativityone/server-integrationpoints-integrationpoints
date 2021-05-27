using kCura.IntegrationPoints.Data;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync
{
	public class SyncConfigurationService : ISyncConfigurationService
	{	
		private readonly IHelper _helper;
		private readonly IAPILog _log;

		public SyncConfigurationService(IHelper helper, IAPILog log)
		{
			_helper = helper;
			_log = log;
		}

		public async Task<int?> TryGetResumedSyncConfigurationIdAsync(int workspaceId, int jobHistoryId)
		{
			using (IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				bool syncConfigurationRdoExists = await CheckSyncConfigurationRdoExistenceAsync(objectManager, workspaceId).ConfigureAwait(false);

				return syncConfigurationRdoExists
					? await QueryForExistingSyncConfigurationAsync(objectManager, workspaceId, jobHistoryId).ConfigureAwait(false)
					: null;
			}
		}

		private static async Task<bool> CheckSyncConfigurationRdoExistenceAsync(IObjectManager objectManager, int workspaceId)
		{
			QueryRequest syncConfigurationRdoExistsRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.ObjectType },
				Condition = $"'Name' == '{ObjectTypes.SyncConfiguration}'"
			};

			QueryResultSlim syncConfigurationExistsResult = await objectManager.QuerySlimAsync(
					workspaceId, syncConfigurationRdoExistsRequest, 0, 1)
				.ConfigureAwait(false);

			return syncConfigurationExistsResult.Objects.Count > 0;
		}

		private async Task<int?> QueryForExistingSyncConfigurationAsync(IObjectManager objectManager, int workspaceId, int jobHistoryId)
		{
			QueryRequest syncConfigurationExistsForJobHistoryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.SyncConfigurationGuid },
				Condition = $"'JobHistoryId' == {jobHistoryId}"
			};

			QueryResultSlim result = await objectManager.QuerySlimAsync(
					workspaceId, syncConfigurationExistsForJobHistoryRequest, 0, int.MaxValue)
				.ConfigureAwait(false);
			if (result.Objects.Count == 1)
			{
				return result.Objects.Single().ArtifactID;
			}

			if (result.Objects.Count == 0)
			{
				return null;
			}

			_log.LogWarning("For JobHistory {jobHistory} has been found {count} Sync Configurations. " +
							"System create new Sync Configuration instance", jobHistoryId, result.Objects.Count);
			return null;
		}
	}
}