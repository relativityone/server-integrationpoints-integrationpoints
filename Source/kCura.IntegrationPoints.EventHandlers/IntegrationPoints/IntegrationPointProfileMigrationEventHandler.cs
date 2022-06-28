using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
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
		private readonly IRepositoryFactory _repositoryFactory;

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => "Failed to migrate the Integration Point Profiles.";

		internal const string _profilesDoNotExistInCreatedWorkspaceMessageTemplate_Migration = @"Following profiles could not be migrated, because they don't exist in created workspace ({workspaceId}): {profiles}";
		internal const string _profilesDoNotExistInCreatedWorkspaceMessageTemplate_Deletion = @"Following profiles could not be deleted, because they don't exist in created workspace ({workspaceId}): {profiles}";


		public IntegrationPointProfileMigrationEventHandler()
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(Helper));
			Func<int, IRelativityObjectManager> createRelativityObjectManager = CreateRelativityObjectManager;
			var objectArtifactIdsByStringFieldValueQuery = new ObjectArtifactIdsByStringFieldValueQuery(createRelativityObjectManager);
			_integrationPointProfilesQuery = new IntegrationPointProfilesQuery(createRelativityObjectManager, objectArtifactIdsByStringFieldValueQuery);
			_repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
		}

		internal IntegrationPointProfileMigrationEventHandler(IErrorService errorService,
			Func<IRelativityObjectManagerFactory> relativityObjectManagerFactoryProvider,
			IIntegrationPointProfilesQuery integrationPointProfilesQuery,
			IRepositoryFactory repositoryFactory) : base(errorService)
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(relativityObjectManagerFactoryProvider);
			_integrationPointProfilesQuery = integrationPointProfilesQuery;
			_repositoryFactory = repositoryFactory;
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

			var profilesInCreatedWorkspace = new HashSet<int>(await _integrationPointProfilesQuery.CheckIfProfilesExistAsync(WorkspaceID, allProfiles.Select(x => x.ArtifactId)).ConfigureAwait(false));


			profilesToPreserve = CheckProfilesExistInWorkspace(profilesToPreserve, profilesInCreatedWorkspace, _profilesDoNotExistInCreatedWorkspaceMessageTemplate_Migration);
			profilesToDelete = CheckProfilesExistInWorkspace(profilesToDelete, profilesInCreatedWorkspace, _profilesDoNotExistInCreatedWorkspaceMessageTemplate_Deletion);

			await Task.WhenAll(
					DeleteProfilesAsync(profilesToDelete.Select(x => x.ArtifactId).ToList()),
					UpdateProfilesAsync(profilesToPreserve)
				).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks if profile was copied over form template workspace to prevent crashing
		///
		/// https://jira.kcura.com/browse/REL-411909
		/// </summary>
		/// <param name="profilesFromTemplateWorkspace"></param>
		/// <returns>Profiles loaded from created workspace selected by ArtifactId</returns>
		private List<IntegrationPointProfile> CheckProfilesExistInWorkspace(List<IntegrationPointProfile> profilesToCheck, HashSet<int> allExitsingProfilesArtifactIds, string messageTemplate)
		{
			List<IntegrationPointProfile> existingProfiles = new List<IntegrationPointProfile>();
			List<int> missingProfiles = new List<int>();

			foreach (var profile in profilesToCheck)
			{
				if (allExitsingProfilesArtifactIds.Contains(profile.ArtifactId))
				{
					existingProfiles.Add(profile);
				}
				else
				{
					missingProfiles.Add(profile.ArtifactId);
				}
			}

			if (existingProfiles.Count != profilesToCheck.Count)
			{
				Logger.LogWarning(messageTemplate, WorkspaceID, missingProfiles);
			}

			return existingProfiles;
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
				bool success = await objectManager.MassUpdateAsync(new[] { profile.ArtifactId }, fieldsToUpdate, FieldUpdateBehavior.Replace).ConfigureAwait(false);

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
            sourceConfiguration[nameof(SourceConfiguration.SourceViewId)] = 0;
            int templateSearchId = sourceConfiguration[nameof(SourceConfiguration.SavedSearchArtifactId)].ToObject<int>();
            sourceConfiguration[nameof(SourceConfiguration.SavedSearchArtifactId)] = TryMatchSavedSearchArtifactId(templateSearchId);
            sourceConfiguration[nameof(SourceConfiguration.SourceWorkspaceArtifactId)] = WorkspaceID;
			sourceConfiguration[destinationFolderArtifactIdPropertyName] = null;
			string updatedSourceConfiguration = sourceConfiguration.ToString(Formatting.None);
			return updatedSourceConfiguration;
		}

		private int TryMatchSavedSearchArtifactId(int templateSearchId)
		{
			if (templateSearchId > 0)
			{
				ISavedSearchQueryRepository templateSearchQueryRepository = _repositoryFactory.GetSavedSearchQueryRepository(TemplateWorkspaceID);
				SavedSearchDTO templateSearch = templateSearchQueryRepository.RetrieveSavedSearch(templateSearchId);
				if (templateSearch != null)
				{
					ISavedSearchQueryRepository destinationSearchQueryRepository = _repositoryFactory.GetSavedSearchQueryRepository(WorkspaceID);
					IEnumerable<SavedSearchDTO> destinationSearches = destinationSearchQueryRepository.RetrievePublicSavedSearches();
					SavedSearchDTO matchedSearch = destinationSearches.FirstOrDefault(x => x.Name == templateSearch.Name);
					return matchedSearch?.ArtifactId ?? 0;
				}
			}

			return 0;
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
