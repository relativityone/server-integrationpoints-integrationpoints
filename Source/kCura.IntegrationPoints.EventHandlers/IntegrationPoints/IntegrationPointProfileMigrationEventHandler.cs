using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Exceptions;
using Polly;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private const int _MAX_RETRIES = 3;
		private const double _EXPONENTIAL_SLEEP_BASE = 2;

		private readonly Guid _integrationPointProfileObjectTypeGuid = Guid.Parse("6DC915A9-25D7-4500-97F7-07CB98A06F64");
		private readonly Guid _destinationProviderObjectTypeGuid = Guid.Parse("d014f00d-f2c0-4e7a-b335-84fcb6eae980");
		private readonly Guid _sourceProviderObjectTypeGuid = Guid.Parse("5BE4A1F7-87A8-4CBE-A53F-5027D4F70B80");

		private readonly Guid _destinationProviderFieldOnProfileObjectGuid = Guid.Parse("7d9e7944-bf13-4c4f-a9eb-5f60e683ec0c");
		private readonly Guid _sourceProviderFieldOnProfileObjectGuid = Guid.Parse("60d3de54-f0d5-4744-a23f-a17609edc537");
		private readonly Guid _identifierFieldOnDestinationProviderObjectGuid = Guid.Parse("9fa104ac-13ea-4868-b716-17d6d786c77a");
		private readonly Guid _identifierFieldOnSourceProviderObjectGuid = Guid.Parse("d0ecc6c9-472c-4296-83e1-0906f0c0fbb9");

		private readonly Guid _relativityDestinationProviderTypeGuid = Guid.Parse("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
		private readonly Guid _relativitySourceProviderTypeGuid = Guid.Parse("423b4d43-eae9-4e14-b767-17d629de4bb2");

		private readonly Func<int, TimeSpan> _sleepDurationProvider = i => TimeSpan.FromSeconds(Math.Pow(_EXPONENTIAL_SLEEP_BASE, i));

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => $"Failed to migrate the Integration Point Profiles, because: {ex.Message}";


		public IntegrationPointProfileMigrationEventHandler()
		{
		}

		public IntegrationPointProfileMigrationEventHandler(IErrorService errorService, Func<int, TimeSpan> sleepDurationProvider) : base(errorService)
		{
			_sleepDurationProvider = sleepDurationProvider;
		}

		protected override void Run()
		{
			Policy
				.Handle<Exception>()
				.WaitAndRetryAsync(_MAX_RETRIES, _sleepDurationProvider, (exception, timeSpan, retryCount, ctx) =>
					Logger.LogWarning(exception, "Migration of the Integration Point Profiles failed for {n} time. Waiting for {seconds} seconds.",
						retryCount, timeSpan.TotalSeconds))
				.ExecuteAsync(MigrateProfiles)
				.GetAwaiter()
				.GetResult();
		}

		private async Task MigrateProfiles()
		{
			using (IObjectManager objectManager = CreateObjectManager())
			{
				int syncDestinationProviderArtifactId = await GetSyncDestinationProviderArtifactIdInTemplateWorkspace(objectManager).ConfigureAwait(false);
				int syncSourceProviderArtifactId = await GetSyncSourceProviderArtifactIdInTemplateWorkspace(objectManager).ConfigureAwait(false);

				List<int> nonSyncProfilesArtifactIds = await GetNonSyncProfilesArtifactIdsFromTemplateWorkspace(syncDestinationProviderArtifactId, syncSourceProviderArtifactId, objectManager).ConfigureAwait(false);
				if (nonSyncProfilesArtifactIds.Any())
				{
					await DeleteNonSyncProfilesInCreatedWorkspace(nonSyncProfilesArtifactIds, objectManager).ConfigureAwait(false);
				}
			}
		}

		private async Task<int> GetSyncDestinationProviderArtifactIdInTemplateWorkspace(IObjectManager objectManager) =>
			await GetObjectArtifactIdByGuidFieldValue(objectManager, _destinationProviderObjectTypeGuid, _identifierFieldOnDestinationProviderObjectGuid, _relativityDestinationProviderTypeGuid)
				.ConfigureAwait(false);

		private async Task<int> GetSyncSourceProviderArtifactIdInTemplateWorkspace(IObjectManager objectManager) =>
			await GetObjectArtifactIdByGuidFieldValue(objectManager, _sourceProviderObjectTypeGuid, _identifierFieldOnSourceProviderObjectGuid, _relativitySourceProviderTypeGuid)
				.ConfigureAwait(false);

		private async Task<int> GetObjectArtifactIdByGuidFieldValue(IObjectManager objectManager, Guid objectTypeGuid, Guid fieldGuid, Guid value)
		{
			Condition searchCondition = new TextCondition(fieldGuid, TextConditionEnum.EqualTo, value.ToString());

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = objectTypeGuid
				},
				Condition = searchCondition.ToQueryString()
			};
			QueryResult queryResult = await objectManager.QueryAsync(TemplateWorkspaceID, queryRequest, 0, 1).ConfigureAwait(false);

			if (queryResult.TotalCount < 1)
			{
				throw new IntegrationPointsException($"Relativity object of type {objectTypeGuid} with field {fieldGuid} of value {value} in workspace {WorkspaceId} was not found");
			}

			int artifactId = queryResult.Objects.First().ArtifactID;
			return artifactId;
		}

		private async Task<List<int>> GetNonSyncProfilesArtifactIdsFromTemplateWorkspace(int syncDestinationProviderArtifactId, int syncSourceProviderArtifactId, IObjectManager objectManager)
		{
			Condition destinationProviderIsRelativity = new ObjectCondition(_destinationProviderFieldOnProfileObjectGuid, ObjectConditionEnum.EqualTo, syncDestinationProviderArtifactId);
			Condition sourceProviderIsRelativity = new ObjectCondition(_sourceProviderFieldOnProfileObjectGuid, ObjectConditionEnum.EqualTo, syncSourceProviderArtifactId);
			Condition deleteNonSyncProfilesCondition = new CompositeCondition(destinationProviderIsRelativity, CompositeConditionEnum.And, sourceProviderIsRelativity).Negate();

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = _integrationPointProfileObjectTypeGuid
				},
				Condition = deleteNonSyncProfilesCondition.ToQueryString()
			};
			QueryResult queryResult = await objectManager.QueryAsync(TemplateWorkspaceID, queryRequest, 0, int.MaxValue).ConfigureAwait(false);

			List<int> nonSyncProfilesArtifactIds = queryResult.Objects.Select(o => o.ArtifactID).ToList();
			return nonSyncProfilesArtifactIds;
		}

		private async Task DeleteNonSyncProfilesInCreatedWorkspace(List<int> nonSyncProfilesArtifactIds, IObjectManager objectManager)
		{
			Condition deleteNonSyncProfilesCondition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In, nonSyncProfilesArtifactIds);

			var massDeleteByCriteriaRequest = new MassDeleteByCriteriaRequest
			{
				ObjectIdentificationCriteria = new ObjectIdentificationCriteria
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = _integrationPointProfileObjectTypeGuid
					},
					Condition = deleteNonSyncProfilesCondition.ToQueryString()
				}
			};
			MassDeleteResult massDeleteResult = await objectManager.DeleteAsync(WorkspaceId, massDeleteByCriteriaRequest).ConfigureAwait(false);

			if (!massDeleteResult.Success)
			{
				throw new IntegrationPointsException("Deleting non Sync Integration Point profiles failed");
			}
		}

		private IObjectManager CreateObjectManager() => Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System);

		private int WorkspaceId => Helper.GetActiveCaseID();
	}
}