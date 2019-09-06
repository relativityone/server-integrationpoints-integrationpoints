using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Domain.Exceptions;
using Polly;
using Polly.Retry;
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
		private readonly Guid _integrationPointProfileObjectTypeGuid = Guid.Parse("6DC915A9-25D7-4500-97F7-07CB98A06F64");
		private readonly Guid _destinationProviderObjectTypeGuid = Guid.Parse("d014f00d-f2c0-4e7a-b335-84fcb6eae980");

		private readonly Guid _destinationProviderFieldOnProfileObjectGuid = Guid.Parse("7d9e7944-bf13-4c4f-a9eb-5f60e683ec0c");
		private readonly Guid _identifierFieldOnDestinationProviderObjectGuid = Guid.Parse("9fa104ac-13ea-4868-b716-17d6d786c77a");

		private readonly Guid _relativityDestinationProviderTypeGuid = Guid.Parse("74A863B9-00EC-4BB7-9B3E-1E22323010C6");

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => $"Failed to migrate the Integration Point Profiles, because: {ex.Message}";

		protected override void Run()
		{
			const int maxRetries = 3;
			const double exponentialWaitBase = 2;

			TimeSpan SleepDurationProvider(int i) =>
				TimeSpan.FromSeconds(Math.Pow(exponentialWaitBase, i));

			void OnRetry(Exception exception, TimeSpan timeSpan, int retryCount, Polly.Context ctx) =>
				Logger.LogWarning(exception, "Migration of the Integration Point Profiles failed for {n} time. Waiting for {seconds} seconds.", retryCount, timeSpan.TotalSeconds);

			Policy
				.Handle<Exception>()
				.WaitAndRetry(maxRetries, SleepDurationProvider, OnRetry)
				.ExecuteAsync(MigrateProfiles)
				.GetAwaiter()
				.GetResult();
		}

		private async Task MigrateProfiles()
		{
			using (IObjectManager objectManager = CreateObjectManager())
			{
				int syncDestinationProviderArtifactId = await GetSyncDestinationProviderArtifactId(objectManager).ConfigureAwait(false);

				await DeleteNonSyncProfiles(syncDestinationProviderArtifactId, objectManager).ConfigureAwait(false);
			}
		}

		private async Task DeleteNonSyncProfiles(int syncDestinationProviderArtifactId, IObjectManager objectManager)
		{
			Condition deleteNonSyncProfilesCondition = new ObjectCondition(_destinationProviderFieldOnProfileObjectGuid, ObjectConditionEnum.EqualTo, syncDestinationProviderArtifactId).Negate();

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

		private async Task<int> GetSyncDestinationProviderArtifactId(IObjectManager objectManager)
		{
			Condition syncDestinationProviderSearchCondition = new TextCondition(_identifierFieldOnDestinationProviderObjectGuid, TextConditionEnum.EqualTo, _relativityDestinationProviderTypeGuid.ToString());

			QueryRequest syncDestinationProviderQueryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = _destinationProviderObjectTypeGuid
				},
				Condition = syncDestinationProviderSearchCondition.ToQueryString()
			};
			QueryResult syncDestinationProviderQueryResult = await objectManager.QueryAsync(WorkspaceId, syncDestinationProviderQueryRequest, 0, 1).ConfigureAwait(false);

			if (syncDestinationProviderQueryResult.TotalCount < 1)
			{
				throw new IntegrationPointsException("Relativity destination provider was not found");
			}

			int syncDestinationProviderArtifactId = syncDestinationProviderQueryResult.Objects.First().ArtifactID;
			return syncDestinationProviderArtifactId;
		}

		private IObjectManager CreateObjectManager() => Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System);

		private int WorkspaceId => Helper.GetActiveCaseID();
	}
}