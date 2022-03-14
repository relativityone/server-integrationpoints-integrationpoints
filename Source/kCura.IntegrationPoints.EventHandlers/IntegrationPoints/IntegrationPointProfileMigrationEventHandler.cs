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
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private readonly Lazy<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;
		private readonly IIntegrationPointProfilesQuery _integrationPointProfilesQuery;

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => "Failed to migrate the Integration Point Profiles.";

		public IntegrationPointProfileMigrationEventHandler()
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(Helper));
			Func<int, IRelativityObjectManager> createRelativityObjectManager = CreateRelativityObjectManager;
			var objectArtifactIdsByStringFieldValueQuery = new ObjectArtifactIdsByStringFieldValueQuery(createRelativityObjectManager);
			_integrationPointProfilesQuery = new IntegrationPointProfilesQuery(createRelativityObjectManager, objectArtifactIdsByStringFieldValueQuery);
		}

		public IntegrationPointProfileMigrationEventHandler(IErrorService errorService,
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
			int sourceProviderArtifactID = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIDAsync(TemplateWorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactID = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIDAsync(TemplateWorkspaceID).ConfigureAwait(false);
			List<IntegrationPointProfile> allProfiles = (await _integrationPointProfilesQuery.GetAllProfilesAsync(TemplateWorkspaceID).ConfigureAwait(false)).ToList();
			List<IntegrationPointProfile> profilesToPreserve = _integrationPointProfilesQuery
				.GetProfilesToUpdate(allProfiles, sourceProviderArtifactID, destinationProviderArtifactID).ToList();
			List<IntegrationPointProfile> profilesToDelete = _integrationPointProfilesQuery
				.GetProfilesToDelete(allProfiles, sourceProviderArtifactID, destinationProviderArtifactID).ToList();

			await Task.WhenAll(
					DeleteProfilesAsync(profilesToDelete.Select(x => x.ArtifactId).ToList()),
					UpdateProfilesAsync(profilesToPreserve)
				).ConfigureAwait(false);
		}

		private async Task DeleteProfilesAsync(IReadOnlyCollection<int> profilesIDs)
		{
			if (profilesIDs.IsNullOrEmpty())
			{
				return;
			}

			bool success = await CreateRelativityObjectManager(WorkspaceID)
				.MassDeleteAsync(profilesIDs)
				.ConfigureAwait(false);
			if (!success)
			{
				throw new IntegrationPointsException("Deleting Integration Point profiles failed.");
			}
		}

		private async Task UpdateProfilesAsync(IReadOnlyCollection<IntegrationPointProfile> profilesToUpdate)
		{
			if (profilesToUpdate.IsNullOrEmpty())
			{
				return;
			}

			IRelativityObjectManager objectManager = CreateRelativityObjectManager(WorkspaceID);

			int sourceProviderArtifactID = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIDAsync(WorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactID = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIDAsync(WorkspaceID).ConfigureAwait(false);
			int integrationPointTypeArtifactID = await _integrationPointProfilesQuery.GetIntegrationPointExportTypeArtifactIDAsync(WorkspaceID).ConfigureAwait(false);

			List<IntegrationPointProfile> failedProfiles = new List<IntegrationPointProfile>();

			foreach (IntegrationPointProfile profile in profilesToUpdate)
			{
				FieldRefValuePair[] fieldsToUpdate = GetFieldsToUpdate(profile, sourceProviderArtifactID, destinationProviderArtifactID, integrationPointTypeArtifactID);
				bool success = await objectManager.MassUpdateAsync(new[] {profile.ArtifactId}, fieldsToUpdate, FieldUpdateBehavior.Replace).ConfigureAwait(false);

				if (!success)
				{
					failedProfiles.Add(profile);
				}
			}

			if (failedProfiles.Any())
			{
				string concatenatedProfiles = string.Join(",", failedProfiles.Select(profile => profile.ArtifactId));
				throw new IntegrationPointsException($"Failed to migrate {failedProfiles.Count} profiles artifact IDs: {concatenatedProfiles}");
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
					Field = new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.DestinationConfigurationGuid
					},
					Value = UpdateDestinationConfiguration(profile.DestinationConfiguration)
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
			const string destinationFolderArtifactIdPropertyName = "FolderArtifactId";

			JObject sourceConfiguration = JObject.Parse(sourceConfigurationJson);
			sourceConfiguration[nameof(SourceConfiguration.SavedSearchArtifactId)] = 0;
			sourceConfiguration[nameof(SourceConfiguration.SourceWorkspaceArtifactId)] = WorkspaceID;
			sourceConfiguration[destinationFolderArtifactIdPropertyName] = null;
			string updatedSourceConfiguration = sourceConfiguration.ToString(Formatting.None);
			return updatedSourceConfiguration;
		}

		private string UpdateDestinationConfiguration(string destinationConfigurationJson)
		{
			JObject destinationConfiguration = JObject.Parse(destinationConfigurationJson);
			destinationConfiguration[nameof(ImportSettings.ImagePrecedence)] = null;
			string updatedDestinationConfiguration = destinationConfiguration.ToString(Formatting.None);
			return updatedDestinationConfiguration;
		}
		
		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceID) =>
			_relativityObjectManagerFactory.Value.CreateRelativityObjectManager(workspaceID);

		private int WorkspaceID => Helper.GetActiveCaseID();
	}
}
