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
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Services;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.EventHandlers
{
	[TestFixture]
	public class IntegrationPointProfileMigrationEventHandlerTest
	{
		private ISerializer _serializer;

		private const int _SAVED_SEARCH_ARTIFACT_ID = 123456;

		private readonly LinkedList<Action> _teardownActions = new LinkedList<Action>();
		private readonly IObjectArtifactIdsByStringFieldValueQuery _objectArtifactIdsByStringFieldValueQuery =
			new ObjectArtifactIdsByStringFieldValueQuery(CreateRelativityObjectManagerForWorkspace);

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			_serializer = new JSONSerializer();
			IEnumerable<int> createdProfilesArtifactIds = await CreateTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactID).ConfigureAwait(false);
			_teardownActions.AddLast(() => DeleteTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactID, createdProfilesArtifactIds).GetAwaiter().GetResult());
		}

		private static async Task DeleteTestProfilesAsync(int workspaceID, IEnumerable<int> profilesArtifactIdsToDelete)
		{
			using (var objectManager = SystemTestsSetupFixture.TestHelper.CreateProxy<IObjectManager>())
			{
				var request = new MassDeleteByObjectIdentifiersRequest
				{
					Objects = profilesArtifactIdsToDelete
						.Select(aid => new RelativityObjectRef { ArtifactID = aid })
						.ToList()
				};
				await objectManager.DeleteAsync(workspaceID, request).ConfigureAwait(false);
			}
		}

		[Test]
		public async Task ItShouldCopyOnlySyncProfiles()
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
			SystemTestsSetupFixture.InvokeActionsAndResetFixtureOnException(_teardownActions);
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
			int exportIntegrationPointTypeArtifactID = await GetTypeArtifactIdAsync(targetWorkspaceID, IntegrationPointTypes.ExportName)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.Type.HasValue)
				.ShouldAllBeEquivalentTo(true);
			targetWorkspaceProfiles.Select(p => p.Type)
				.ShouldAllBeEquivalentTo(exportIntegrationPointTypeArtifactID);
			
			List<SourceConfiguration> sourceConfigurations = targetWorkspaceProfiles
				.Select(profile => profile.SourceConfiguration)
				.Select(sourceConfigJson => _serializer.Deserialize<SourceConfiguration>(sourceConfigJson))
				.ToList();

			List<ImportSettings> destinationConfigurations = targetWorkspaceProfiles
				.Select(profile => profile.DestinationConfiguration)
				.Select(destinationConfiguration => _serializer.Deserialize<ImportSettings>(destinationConfiguration))
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
			IEnumerable<int> objectsArtifactIds = await _objectArtifactIdsByStringFieldValueQuery
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
			int importTypeArtifactID = await GetTypeArtifactIdAsync(workspaceID, IntegrationPointTypes.ImportName).ConfigureAwait(false);
			int exportTypeArtifactID = await GetTypeArtifactIdAsync(workspaceID, IntegrationPointTypes.ExportName).ConfigureAwait(false);

			int relativitySourceProviderArtifactID = await GetSourceProviderArtifactIdAsync(workspaceID, SourceProviders.RELATIVITY).ConfigureAwait(false);
			int loadFileSourceProviderArtifactID = await GetSourceProviderArtifactIdAsync(workspaceID, SourceProviders.IMPORTLOADFILE).ConfigureAwait(false);
			int ftpSourceProviderArtifactID = await GetSourceProviderArtifactIdAsync(workspaceID, SourceProviders.FTP).ConfigureAwait(false);
			int ldapSourceProviderArtifactID = await GetSourceProviderArtifactIdAsync(workspaceID, SourceProviders.LDAP).ConfigureAwait(false);

			int relativityDestinationProviderArtifactID = await GetDestinationProviderArtifactIdAsync(workspaceID, DestinationProviders.RELATIVITY).ConfigureAwait(false);
			int loadFileDestinationProviderArtifactID = await GetDestinationProviderArtifactIdAsync(workspaceID, DestinationProviders.LOADFILE).ConfigureAwait(false);

			List<IntegrationPointProfile> profiles = new List<IntegrationPointProfile>()
			{
				// Sync profile
				new IntegrationPointProfile
				{
					Name = "Sync profile",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
					DestinationConfiguration = CreateDestinationConfiguration(exportToProduction: false)
				},

				// Non-Sync profiles
				new IntegrationPointProfile
				{
					Name = "prod to prod",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfiguration(SourceConfiguration.ExportType.ProductionSet),
					DestinationConfiguration = CreateDestinationConfiguration(exportToProduction: true)
				},
				new IntegrationPointProfile
				{
					Name = "savedsearch to prod",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfiguration(SourceConfiguration.ExportType.SavedSearch),
					DestinationConfiguration = CreateDestinationConfiguration(exportToProduction: true)
				},
				new IntegrationPointProfile
				{
					Name = "prod to folder",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
					SourceConfiguration = CreateSourceConfiguration(SourceConfiguration.ExportType.ProductionSet),
					DestinationConfiguration = CreateDestinationConfiguration(exportToProduction: false)
				},
				new IntegrationPointProfile
				{
					Name = "export to load file",
					Type = exportTypeArtifactID,
					SourceProvider = relativitySourceProviderArtifactID,
					DestinationProvider = loadFileDestinationProviderArtifactID,
				},
				new IntegrationPointProfile
				{
					Name = "import from ftp",
					Type = importTypeArtifactID,
					SourceProvider = ftpSourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID
				},
				new IntegrationPointProfile
				{
					Name = "import from load file",
					Type = importTypeArtifactID,
					SourceProvider = loadFileSourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID,
				},
				new IntegrationPointProfile
				{
					Name = "import from ldap",
					Type = importTypeArtifactID,
					SourceProvider = ldapSourceProviderArtifactID,
					DestinationProvider = relativityDestinationProviderArtifactID
				}
			};

			return profiles;
		}

		private static async Task<int> GetTypeArtifactIdAsync(int workspaceID, string typeName)
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

		private static async Task<int> GetSourceProviderArtifactIdAsync(int workspaceID, string guid)
		{
			using (var providerClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IProviderManager>())
			{
				return await providerClient.GetSourceProviderArtifactIdAsync(workspaceID, guid).ConfigureAwait(false);
			}
		}

		private static async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceID, string guid)
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

		private string CreateSourceConfiguration(SourceConfiguration.ExportType exportType)
		{
			var sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = SystemTestsSetupFixture.SourceWorkspace.ArtifactID,
				SavedSearchArtifactId = _SAVED_SEARCH_ARTIFACT_ID,
				TypeOfExport = exportType
			};
			return _serializer.Serialize(sourceConfiguration);
		}

		private string CreateDestinationConfiguration(bool exportToProduction)
		{
			ImportSettings destinationConfiguration = new ImportSettings()
			{
				ProductionImport = exportToProduction
			};
			return _serializer.Serialize(destinationConfiguration);
		}
	}
}