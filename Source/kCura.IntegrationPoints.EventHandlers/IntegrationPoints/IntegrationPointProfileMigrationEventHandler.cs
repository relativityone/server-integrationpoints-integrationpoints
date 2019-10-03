using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private readonly Lazy<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;
		private readonly IIntegrationPointProfilesQuery _integrationPointProfilesQuery;
		private readonly ISerializer _serializer;

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => "Failed to migrate the Integration Point Profiles.";

		public IntegrationPointProfileMigrationEventHandler()
		{
			_serializer = new JSONSerializer();
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(Helper));
			Func<int, IRelativityObjectManager> createRelativityObjectManager = CreateRelativityObjectManager;
			var objectArtifactIdsByStringFieldValueQuery = new ObjectArtifactIdsByStringFieldValueQuery(createRelativityObjectManager);
			_integrationPointProfilesQuery = new IntegrationPointProfilesQuery(createRelativityObjectManager, objectArtifactIdsByStringFieldValueQuery);
		}

		internal IntegrationPointProfileMigrationEventHandler(IErrorService errorService,
			Func<IRelativityObjectManagerFactory> relativityObjectManagerFactoryProvider,
			IIntegrationPointProfilesQuery integrationPointProfilesQuery) : base(errorService)
		{
			_serializer = new JSONSerializer();
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(relativityObjectManagerFactoryProvider);
			_integrationPointProfilesQuery = integrationPointProfilesQuery;
		}

		protected override void Run()
		{
			MigrateProfilesAsync().GetAwaiter().GetResult();
		}

		private async Task MigrateProfilesAsync()
		{
			int sourceProviderArtifactID = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIDAsync(TemplateWorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactID = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIDAsync(TemplateWorkspaceID).ConfigureAwait(false);
			List<IntegrationPointProfile> allProfiles = (await _integrationPointProfilesQuery.GetAllProfilesAsync(TemplateWorkspaceID).ConfigureAwait(false)).ToList();
			List<IntegrationPointProfile> syncProfiles = _integrationPointProfilesQuery
				.GetSyncProfiles(allProfiles, sourceProviderArtifactID, destinationProviderArtifactID).ToList();
			List<IntegrationPointProfile> nonSyncProfiles = _integrationPointProfilesQuery
				.GetNonSyncProfiles(allProfiles, sourceProviderArtifactID, destinationProviderArtifactID).ToList();

			await Task.WhenAll(
					DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(nonSyncProfiles.Select(x => x.ArtifactId).ToList()),
					ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(syncProfiles)
				).ConfigureAwait(false);
		}

		private async Task DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(IReadOnlyCollection<int> nonSyncProfilesIDs)
		{
			if (nonSyncProfilesIDs.IsNullOrEmpty())
			{
				return;
			}

			bool success = await CreateRelativityObjectManager(WorkspaceID)
				.MassDeleteAsync(nonSyncProfilesIDs)
				.ConfigureAwait(false);
			if (!success)
			{
				throw new IntegrationPointsException("Deleting non Sync Integration Point profiles failed");
			}
		}

		private async Task ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(IReadOnlyCollection<IntegrationPointProfile> syncProfiles)
		{
			if (syncProfiles.IsNullOrEmpty())
			{
				return;
			}

			IRelativityObjectManager objectManager = CreateRelativityObjectManager(WorkspaceID);

			int sourceProviderArtifactID = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIDAsync(WorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactID = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIDAsync(WorkspaceID).ConfigureAwait(false);
			int integrationPointTypeArtifactID = await _integrationPointProfilesQuery.GetIntegrationPointExportTypeArtifactIDAsync(WorkspaceID).ConfigureAwait(false);

			foreach (IntegrationPointProfile profile in syncProfiles)
			{
				FieldRefValuePair[] fieldsToUpdate = GetFieldsToUpdate(profile, sourceProviderArtifactID, destinationProviderArtifactID, integrationPointTypeArtifactID);
				bool success = await objectManager.MassUpdateAsync(new[] {profile.ArtifactId}, fieldsToUpdate, FieldUpdateBehavior.Replace).ConfigureAwait(false);

				if (!success)
				{
					throw new IntegrationPointsException($"Updating Integration Point Profile ArtifactID: {profile.ArtifactId} Name: {profile.Name} with ObjectManager failed.");
				}
			}
		}

		private FieldRefValuePair[] GetFieldsToUpdate(IntegrationPointProfile profile, int sourceProviderArtifactID, int destinationProviderArtifactID, int integrationPointTypeArtifactID)
		{
			return new[]
			{
				new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid
					},
					Value = UpdateSourceConfiguration(profile.SourceConfiguration)
				},
				new FieldRefValuePair()
				{
					Field = new FieldRef
					{
						Guid = IntegrationPointProfileFieldGuids.SourceProviderGuid
					},
					Value = new RelativityObjectRef()
					{
						ArtifactID = sourceProviderArtifactID
					}
				},
				new FieldRefValuePair()
				{
					Field = new FieldRef
					{
						Guid = IntegrationPointProfileFieldGuids.DestinationProviderGuid
					},
					Value = new RelativityObjectRef()
					{
						ArtifactID = destinationProviderArtifactID
					}
				}, 
				new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.TypeGuid
					},
					Value = new RelativityObjectRef()
					{
						ArtifactID = integrationPointTypeArtifactID
					}
				}
			};
		}

		private string UpdateSourceConfiguration(string sourceConfigurationJson)
		{
			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(sourceConfigurationJson);
			sourceConfiguration.SavedSearchArtifactId = 0;
			sourceConfiguration.SourceWorkspaceArtifactId = WorkspaceID;
			return _serializer.Serialize(sourceConfiguration);
		}
		
		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceID) =>
			_relativityObjectManagerFactory.Value.CreateRelativityObjectManager(workspaceID);

		private int WorkspaceID => Helper.GetActiveCaseID();
	}
}
