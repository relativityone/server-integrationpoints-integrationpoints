using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.IntegrationPoints.Services;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.EventHandlers
{
	[TestFixture]
	public class IntegrationPointProfileMigrationEventHandlerTest
	{
		private ISerializer _serializer;
		private IEnumerable<int> _createdProfilesArtifactIDs;

		private const int _SAVED_SEARCH_ARTIFACT_ID = 123456;

		private readonly IObjectArtifactIdsByStringFieldValueQuery _objectArtifactIDsByStringFieldValueQuery =
			new ObjectArtifactIdsByStringFieldValueQuery(CreateRelativityObjectManagerForWorkspace);

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			_serializer = new JSONSerializer();
			_createdProfilesArtifactIDs = await CreateTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactID).ConfigureAwait(false);
		}

		private static async Task DeleteTestProfilesAsync(int workspaceID, IEnumerable<int> profilesArtifactIDsToDelete)
		{
			using (var objectManager = SystemTestsSetupFixture.TestHelper.CreateProxy<IObjectManager>())
			{
				var request = new MassDeleteByObjectIdentifiersRequest
				{
					Objects = profilesArtifactIDsToDelete
						.Select(aid => new RelativityObjectRef { ArtifactID = aid })
						.ToList()
				};
				await objectManager.DeleteAsync(workspaceID, request).ConfigureAwait(false);
			}
		}

		[IdentifiedTest("bd62499f-9f91-4aea-9330-0ccc7a3bc6e2")]
		public async Task EventHandler_ShouldDeleteAndUpdateExistingProfiles()
		{
			// Act
			TestWorkspace createdWorkspace = await SystemTestsSetupFixture.CreateManagedWorkspaceWithDefaultName(SystemTestsSetupFixture.SourceWorkspace.Name)
				.ConfigureAwait(false);

			// Assert
			await VerifyAllProfilesInDestinationWorkspaceAreSyncOnlyAndHaveProperValuesSetAsync(createdWorkspace.ArtifactID)
				.ConfigureAwait(false);
		}

		[TearDown]
		public void TearDown()
		{
			Action[] actions =
			{
				() => DeleteTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactID, _createdProfilesArtifactIDs).GetAwaiter().GetResult()
			};
			SystemTestsSetupFixture.InvokeActionsAndResetFixtureOnException(actions);
		}

		private async Task VerifyAllProfilesInDestinationWorkspaceAreSyncOnlyAndHaveProperValuesSetAsync(int targetWorkspaceID)
		{
			IRelativityObjectManager objectManager = CreateRelativityObjectManagerForWorkspace(targetWorkspaceID);

			// get all profiles from the created workspace
			List<IntegrationPointProfile> targetWorkspaceProfiles = await objectManager.QueryAsync<IntegrationPointProfile>(new QueryRequest()).ConfigureAwait(false);

			// verify destination provider id
			int syncDestinationProviderArtifactID = await GetSyncDestinationProviderArtifactIDAsync(targetWorkspaceID)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.DestinationProvider)
				.ShouldAllBeEquivalentTo(syncDestinationProviderArtifactID);

			// verify source provider id
			int syncSourceProviderArtifactID = await GetSyncSourceProviderArtifactIDAsync(targetWorkspaceID)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.SourceProvider)
				.ShouldAllBeEquivalentTo(syncSourceProviderArtifactID);

			// verify integration point type id
			int exportIntegrationPointTypeArtifactID = await GetTypeArtifactIDAsync(targetWorkspaceID, IntegrationPointTypes.ExportName)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.Type.HasValue)
				.ShouldAllBeEquivalentTo(true);
			targetWorkspaceProfiles.Select(p => p.Type)
				.ShouldAllBeEquivalentTo(exportIntegrationPointTypeArtifactID);
			
			List<SourceConfiguration> sourceConfigurations = targetWorkspaceProfiles
				.Select(profile => profile.SourceConfiguration)
				.Select(sourceConfigurationJson => _serializer.Deserialize<SourceConfiguration>(sourceConfigurationJson))
				.ToList();

			List<ImportSettings> destinationConfigurations = targetWorkspaceProfiles
				.Select(profile => profile.DestinationConfiguration)
				.Select(destinationConfigurationJson => _serializer.Deserialize<ImportSettings>(destinationConfigurationJson))
				.ToList();

			// verify that source is saved search
			sourceConfigurations.Select(config => config.TypeOfExport)
				.ShouldAllBeEquivalentTo(SourceConfiguration.ExportType.SavedSearch);

			// verify that destination is not production
			destinationConfigurations.Select(config => config.ProductionImport)
				.ShouldAllBeEquivalentTo(false);

			// verify source workspace id in source configuration
			sourceConfigurations.Select(config => config.SourceWorkspaceArtifactId)
				.ShouldAllBeEquivalentTo(targetWorkspaceID);

			// verify saved search id in source configuration
			sourceConfigurations
				.Select(config => config.SavedSearchArtifactId)
				.ShouldAllBeEquivalentTo(0);

			// verify destination folder id in source configuration
			targetWorkspaceProfiles
				.Select(x => JObject.Parse(x.SourceConfiguration)["FolderArtifactId"].Type)
				.ShouldAllBeEquivalentTo(JTokenType.Null);

			// verify image precedence in destination configuration
			targetWorkspaceProfiles
				.Select(x => JObject.Parse(x.DestinationConfiguration)["ImagePrecedence"].Type)
				.ShouldAllBeEquivalentTo(JTokenType.Null);
		}

		private Task<int> GetSyncDestinationProviderArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIDByStringFieldValueAsync<DestinationProvider>(workspaceID,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);
		}

		private Task<int> GetSyncSourceProviderArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIDByStringFieldValueAsync<SourceProvider>(workspaceID,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);
		}

		private async Task<int> GetSingleObjectArtifactIDByStringFieldValueAsync<TSource>(int workspaceId,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			IEnumerable<int> objectsArtifactIds = await _objectArtifactIDsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceId, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIds.Single();
			return artifactId;
		}

		private async Task<IEnumerable<int>> CreateTestProfilesAsync(int workspaceID)
		{
			IRelativityObjectManager objectManager = CreateRelativityObjectManagerForWorkspace(workspaceID);
			List<IntegrationPointProfile> profilesToCreate = await GetProfilesToCreateAsync(workspaceID).ConfigureAwait(false);
			IEnumerable<Task<int>> profilesCreationTasks = profilesToCreate.Select(profile => Task.Run(() => objectManager.Create(profile)));
			int[] profilesArtifactIds = await Task.WhenAll(profilesCreationTasks).ConfigureAwait(false);
			return profilesArtifactIds;
		}

		private async Task<List<IntegrationPointProfile>> GetProfilesToCreateAsync(int workspaceID)
		{
			int importTypeArtifactID = await GetTypeArtifactIDAsync(workspaceID, IntegrationPointTypes.ImportName).ConfigureAwait(false);
			int exportTypeArtifactID = await GetTypeArtifactIDAsync(workspaceID, IntegrationPointTypes.ExportName).ConfigureAwait(false);

			int relativitySourceProviderArtifactID = await GetSourceProviderArtifactIDAsync(workspaceID, SourceProviders.RELATIVITY).ConfigureAwait(false);
			int loadFileSourceProviderArtifactID = await GetSourceProviderArtifactIDAsync(workspaceID, SourceProviders.IMPORTLOADFILE).ConfigureAwait(false);
			int ftpSourceProviderArtifactID = await GetSourceProviderArtifactIDAsync(workspaceID, SourceProviders.FTP).ConfigureAwait(false);
			int ldapSourceProviderArtifactID = await GetSourceProviderArtifactIDAsync(workspaceID, SourceProviders.LDAP).ConfigureAwait(false);

			int relativityDestinationProviderArtifactID = await GetDestinationProviderArtifactIDAsync(workspaceID, DestinationProviders.RELATIVITY).ConfigureAwait(false);
			int loadFileDestinationProviderArtifactID = await GetDestinationProviderArtifactIDAsync(workspaceID, DestinationProviders.LOADFILE).ConfigureAwait(false);

			const string emptyJson = "{}";

			List<IntegrationPointProfile> profiles = new List<IntegrationPointProfile>()
			{
				// Profiles to preserve
				new IntegrationPointProfile
				{
					Name = "Sync saved search to folder",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
					DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false)
				},
				new IntegrationPointProfile()
				{
					Name = "Sync produced images from saved search to folder",
					Type = exportTypeArtifactID,
					SourceProvider =  relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
					DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false, copyImages: true, useImagePrecedence: true)
				},

				// Profiles to delete
				new IntegrationPointProfile
				{
					Name = "prod to prod",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.ProductionSet),
					DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: true)
				},
				new IntegrationPointProfile
				{
					Name = "savedsearch to prod",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.SavedSearch),
					DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: true)
				},
				new IntegrationPointProfile
				{
					Name = "prod to folder",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfigurationJson(SourceConfiguration.ExportType.ProductionSet),
					DestinationConfiguration = CreateDestinationConfigurationJson(exportToProduction: false)
				},
				new IntegrationPointProfile
				{
					Name = "export to load file",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = loadFileDestinationProviderArtifactID,
					SourceConfiguration = emptyJson,
					DestinationConfiguration = emptyJson
				},
				new IntegrationPointProfile
				{
					Name = "import from ftp",
					Type = importTypeArtifactID,
					SourceProvider = ftpSourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = emptyJson,
					DestinationConfiguration = emptyJson
				},
				new IntegrationPointProfile
				{
					Name = "import from load file",
					Type = importTypeArtifactID,
					SourceProvider = loadFileSourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = emptyJson,
					DestinationConfiguration = emptyJson
				},
				new IntegrationPointProfile
				{
					Name = "import from ldap",
					Type = importTypeArtifactID,
					SourceProvider = ldapSourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = emptyJson,
					DestinationConfiguration = emptyJson
				}
			};

			return profiles;
		}

		private static async Task<int> GetTypeArtifactIDAsync(int workspaceID, string typeName)
		{
			using (var typeClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IIntegrationPointTypeManager>())
			{
				IList<IntegrationPointTypeModel> integrationPointTypes = await typeClient.GetIntegrationPointTypes(workspaceID).ConfigureAwait(false);

				IntegrationPointTypeModel foundIntegrationPointType = integrationPointTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
				if (foundIntegrationPointType == null)
				{
					throw new TestException($"Could not find {nameof(IntegrationPointTypeModel)} of name {typeName} in workspace {workspaceID}");
				}
				return foundIntegrationPointType.ArtifactId;
			}
		}

		private static async Task<int> GetSourceProviderArtifactIDAsync(int workspaceID, string guid)
		{
			using (var providerClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IProviderManager>())
			{
				return await providerClient.GetSourceProviderArtifactIdAsync(workspaceID, guid).ConfigureAwait(false);
			}
		}

		private static async Task<int> GetDestinationProviderArtifactIDAsync(int workspaceID, string guid)
		{
			using (var providerClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IProviderManager>())
			{
				return await providerClient.GetDestinationProviderArtifactIdAsync(workspaceID, guid).ConfigureAwait(false);
			}
		}

		private static IRelativityObjectManager CreateRelativityObjectManagerForWorkspace(int workspaceID)
		{
			return SystemTestsSetupFixture.Container.Resolve<IRelativityObjectManagerFactory>().CreateRelativityObjectManager(workspaceID);
		}

		private string CreateSourceConfigurationJson(SourceConfiguration.ExportType exportType)
		{
			var sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = SystemTestsSetupFixture.SourceWorkspace.ArtifactID,
				SavedSearchArtifactId = _SAVED_SEARCH_ARTIFACT_ID,
				TypeOfExport = exportType
			};
			return _serializer.Serialize(sourceConfiguration);
		}

		private string CreateDestinationConfigurationJson(bool exportToProduction = false, bool copyImages = false, bool useImagePrecedence = false)
		{
			JObject configuration = new JObject
			{
				["ProductionImport"] = exportToProduction,
				["ImageImport"] = copyImages
			};

			if (useImagePrecedence)
			{
				configuration["ImagePrecedence"] = new JArray()
				{
					new JObject()
					{
						["displayName"] = "production 1",
						["artifactID"] = 123
					}
				};
			}

			return configuration.ToString();
		}
	}
}