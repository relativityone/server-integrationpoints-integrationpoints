using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private readonly Lazy<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;
		private readonly IIntegrationPointProfilesQuery _integrationPointProfilesQuery;

		private static readonly Guid _destinationProviderObjectTypeGuid = Guid.Parse("d014f00d-f2c0-4e7a-b335-84fcb6eae980");
		private static readonly Guid _identifierFieldOnDestinationProviderObjectGuid = Guid.Parse("9fa104ac-13ea-4868-b716-17d6d786c77a");
		private static readonly Guid _relativityDestinationProviderTypeGuid = Guid.Parse("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
		private static readonly Guid _sourceProviderObjectTypeGuid = Guid.Parse("5BE4A1F7-87A8-4CBE-A53F-5027D4F70B80");
		private static readonly Guid _identifierFieldOnSourceProviderObjectGuid = Guid.Parse("d0ecc6c9-472c-4296-83e1-0906f0c0fbb9");
		private static readonly Guid _relativitySourceProviderTypeGuid = Guid.Parse("423b4d43-eae9-4e14-b767-17d629de4bb2");

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => "Failed to migrate the Integration Point Profiles.";

		public IntegrationPointProfileMigrationEventHandler()
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(Helper));
			Func<int, IRelativityObjectManager> createRelativityObjectManager = CreateRelativityObjectManager;
			var objectArtifactIdsByStringFieldValueQuery = new ObjectArtifactIdsByStringFieldValueQuery(createRelativityObjectManager);
			_integrationPointProfilesQuery = new IntegrationPointProfilesQuery(createRelativityObjectManager, objectArtifactIdsByStringFieldValueQuery);
		}

		internal IntegrationPointProfileMigrationEventHandler(IErrorService errorService,
			Func<IRelativityObjectManagerFactory> relativityObjectManagerFactoryProvider,
			IIntegrationPointProfilesQuery integrationPointProfilesQuery) : base(errorService)
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(relativityObjectManagerFactoryProvider);
			_integrationPointProfilesQuery = integrationPointProfilesQuery;
		}

		protected override void Run()
		{
			MigrateProfilesAsync().GetAwaiter().GetResult();
		}

		private async Task MigrateProfilesAsync()
		{
			(List<int> nonSyncProfilesArtifactIds, List<int> syncProfilesArtifactIds) = await _integrationPointProfilesQuery
				.GetSyncAndNonSyncProfilesArtifactIdsAsync(TemplateWorkspaceID)
				.ConfigureAwait(false);

			await Task.WhenAll(
					DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(nonSyncProfilesArtifactIds),
					ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(syncProfilesArtifactIds)
				).ConfigureAwait(false);
		}

		private async Task DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(IReadOnlyCollection<int> nonSyncProfilesArtifactIds)
		{
			if (nonSyncProfilesArtifactIds.IsNullOrEmpty())
			{
				return;
			}

			bool success = await CreateRelativityObjectManager(WorkspaceId)
				.MassDeleteAsync(nonSyncProfilesArtifactIds)
				.ConfigureAwait(false);
			if (!success)
			{
				throw new IntegrationPointsException("Deleting non Sync Integration Point profiles failed");
			}
		}

		private async Task ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(IEnumerable<int> syncProfilesArtifactIds)
		{
			if (syncProfilesArtifactIds.IsNullOrEmpty())
			{
				return;
			}

			FieldRef destinationProviderField = new FieldRef
			{
				Guid = IntegrationPointProfileFieldGuids.DestinationProviderGuid
			};

			FieldRef sourceProviderField = new FieldRef
			{
				Guid = IntegrationPointProfileFieldGuids.SourceProviderGuid
			};
			FieldRef typeField = new FieldRef()
			{
				Guid = IntegrationPointProfileFieldGuids.TypeGuid
			};
			FieldRef sourceConfigurationField = new FieldRef()
			{
				Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid
			};

			var queryRequest = new QueryRequest
			{
				Fields = new[]
				{
					destinationProviderField,
					sourceProviderField,
					typeField,
					sourceConfigurationField
				}
			};

			IRelativityObjectManager objectManager = CreateRelativityObjectManager(WorkspaceId);
			List<IntegrationPointProfile> integrationPointProfiles = await objectManager
				.QueryAsync<IntegrationPointProfile>(queryRequest)
				.ConfigureAwait(false);

			int sourceProviderArtifactId = await GetSourceProviderArtifactIdAsync(objectManager).ConfigureAwait(false);
			int destinationProviderArtifactId = await GetDestinationProviderArtifactIdAsync(objectManager).ConfigureAwait(false);
			int integrationPointTypeArtifactId = await GetIntegrationPointTypeArtifactIdAsync(objectManager).ConfigureAwait(false);

			foreach (IntegrationPointProfile profile in integrationPointProfiles)
			{
				await objectManager.MassUpdateAsync(new[] {profile.ArtifactId}, new[]
				{
					new FieldRefValuePair()
					{
						Field = sourceConfigurationField,
						Value = UpdateSourceConfiguration(profile.SourceConfiguration)
					},
					new FieldRefValuePair()
					{
						Field = sourceProviderField,
						Value = sourceProviderArtifactId
					},
					new FieldRefValuePair()
					{
						Field = destinationProviderField,
						Value = destinationProviderArtifactId
					}, 
					new FieldRefValuePair()
					{
						Field = typeField,
						Value = integrationPointTypeArtifactId
					}
				}, FieldUpdateBehavior.Replace).ConfigureAwait(false);
			}
		}

		private string UpdateSourceConfiguration(string sourceConfiguration)
		{
			JObject configJson = JObject.Parse(sourceConfiguration);
			configJson.Property("SavedSearchArtifactId").Value = null;
			configJson.Property("SourceWorkspaceArtifactId").Value = WorkspaceId;
			return configJson.ToString();
		}

		private async Task<int> GetSourceProviderArtifactIdAsync(IRelativityObjectManager objectManager)
		{
			return await GetArtifactIdByGuidAsync(objectManager, _sourceProviderObjectTypeGuid, _identifierFieldOnSourceProviderObjectGuid, _relativitySourceProviderTypeGuid).ConfigureAwait(false);
		}

		private async Task<int> GetDestinationProviderArtifactIdAsync(IRelativityObjectManager objectManager)
		{
			return await GetArtifactIdByGuidAsync(objectManager, _destinationProviderObjectTypeGuid, _identifierFieldOnDestinationProviderObjectGuid, _relativityDestinationProviderTypeGuid).ConfigureAwait(false);
		}

		private async Task<int> GetIntegrationPointTypeArtifactIdAsync(IRelativityObjectManager objectManager)
		{
			return await GetArtifactIdByGuidAsync(objectManager, ObjectTypeGuids.IntegrationPointTypeGuid, IntegrationPointTypeFieldGuids.IdentifierGuid,
				Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ConfigureAwait(false);
		}

		private async Task<int> GetArtifactIdByGuidAsync(IRelativityObjectManager objectManager, Guid objectTypeGuid, Guid fieldGuid, Guid value)
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

			ResultSet<RelativityObject> queryResult = await objectManager.QueryAsync(queryRequest, 0, 1).ConfigureAwait(false);

			if (queryResult.TotalCount < 1)
			{
				throw new IntegrationPointsException($"Relativity object type {objectTypeGuid} with field {fieldGuid} of value {value} in workspace of id {WorkspaceId} was not found");
			}

			int artifactId = queryResult.Items.First().ArtifactID;
			return artifactId;
		}

		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceId) =>
			_relativityObjectManagerFactory.Value.CreateRelativityObjectManager(workspaceId);

		private int WorkspaceId => Helper.GetActiveCaseID();
	}
}
