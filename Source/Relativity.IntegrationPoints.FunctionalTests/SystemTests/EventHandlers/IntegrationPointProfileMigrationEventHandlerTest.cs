using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
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
		private const int _SAVED_SEARCH_ARTIFACT_ID = 123456;

		private static readonly IEnumerable<ProfileConfig> _testProfilesConfigs = new[]
		{
			new ProfileConfig
			{
				TypeName = IntegrationPointTypes.ExportName,
				SourceProviderGuid = SourceProviders.RELATIVITY,
				DestinationProviderGuid = DestinationProviders.RELATIVITY
			},
			new ProfileConfig
			{
				TypeName = IntegrationPointTypes.ExportName,
				SourceProviderGuid = SourceProviders.RELATIVITY,
				DestinationProviderGuid = DestinationProviders.LOADFILE,
			},
			new ProfileConfig
			{
				TypeName = IntegrationPointTypes.ImportName,
				SourceProviderGuid = SourceProviders.FTP,
				DestinationProviderGuid = DestinationProviders.RELATIVITY
			},
			new ProfileConfig
			{
				TypeName = IntegrationPointTypes.ImportName,
				SourceProviderGuid = SourceProviders.IMPORTLOADFILE,
				DestinationProviderGuid = DestinationProviders.RELATIVITY
			},
			new ProfileConfig
			{
				TypeName = IntegrationPointTypes.ImportName,
				SourceProviderGuid = SourceProviders.LDAP,
				DestinationProviderGuid = DestinationProviders.RELATIVITY
			}
		};

		private readonly LinkedList<Action> _teardownActions = new LinkedList<Action>();
		private readonly IObjectArtifactIdsByStringFieldValueQuery _objectArtifactIdsByStringFieldValueQuery =
			new ObjectArtifactIdsByStringFieldValueQuery(CreateRelativityObjectManagerForWorkspace);

		[Test]
		public async Task ItShouldCopyOnlySyncProfiles()
		{
			List<int> createdProfilesArtifactIds = await CreateTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactID).ConfigureAwait(false);
			_teardownActions.AddLast(() => DeleteTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactID, createdProfilesArtifactIds).GetAwaiter().GetResult());

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
			var objectManager = CreateRelativityObjectManagerForWorkspace(targetWorkspaceID);

			// get all profiles from the created workspace
			var targetWorkspaceProfiles = await objectManager.QueryAsync<IntegrationPointProfile>(new QueryRequest())
					.ConfigureAwait(false);
			int expectedSyncProfilesCount = _testProfilesConfigs.Count(IsSyncProfile);
			targetWorkspaceProfiles.Should().HaveCount(expectedSyncProfilesCount);

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

			var sourceConfigurations = targetWorkspaceProfiles.Select(p => p.SourceConfiguration)
				.Select(JObject.Parse)
				.ToList();
			// verify source workspace id in source configuration
			sourceConfigurations.Select(c => c[nameof(kCura.IntegrationPoints.Services.RelativityProviderSourceConfiguration.SourceWorkspaceArtifactId)])
				.Select(t => t.ToObject<int>())
				.ShouldAllBeEquivalentTo(targetWorkspaceID);

			// verify saved search id in source configuration
			sourceConfigurations
				.Select(c => c[nameof(kCura.IntegrationPoints.Services.RelativityProviderSourceConfiguration.SavedSearchArtifactId)].Type)
				.ShouldAllBeEquivalentTo(JTokenType.Null);
		}

		private Task<int> GetSyncDestinationProviderArtifactIDAsync(int workspaceID) =>
			GetSingleObjectArtifactIDByStringFieldValueAsync<DestinationProvider>(workspaceID,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

		private Task<int> GetSyncSourceProviderArtifactIDAsync(int workspaceID) =>
			GetSingleObjectArtifactIDByStringFieldValueAsync<SourceProvider>(workspaceID,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

		private async Task<int> GetSingleObjectArtifactIDByStringFieldValueAsync<TSource>(int workspaceId,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			IEnumerable<int> objectsArtifactIds = await _objectArtifactIdsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceId, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIds.Single();
			return artifactId;
		}

		private static async Task<List<int>> CreateTestProfilesAsync(int workspaceID)
		{
			var objectManager = CreateRelativityObjectManagerForWorkspace(workspaceID);

			IList<Task<IntegrationPointProfile>> profileCreationTasks = _testProfilesConfigs
				.Zip(Enumerable.Range(0, _testProfilesConfigs.Count()), (config, i) => new { Config = config, Number = i })
				.Select(x => CreateProfileRdoAsync(workspaceID, x.Number, x.Config))
				.ToList();

			await Task.WhenAll(profileCreationTasks).ConfigureAwait(false);

			var createdProfilesArtifactIds = profileCreationTasks
				.Select(t => t.Result)
				.Select(p => objectManager.Create(p))
				.ToList();

			return createdProfilesArtifactIds;
		}

		private static async Task<IntegrationPointProfile> CreateProfileRdoAsync(int workspaceID, int profileNumber, ProfileConfig config)
		{
			var integrationPointProfile = new IntegrationPointProfile
			{
				Name = $"Profile{profileNumber}",
				SourceProvider = await GetSourceProviderArtifactIdAsync(workspaceID, config.SourceProviderGuid).ConfigureAwait(false),
				DestinationProvider = await GetDestinationProviderArtifactIdAsync(workspaceID, config.DestinationProviderGuid).ConfigureAwait(false),
				Type = await GetTypeArtifactIdAsync(workspaceID, config.TypeName).ConfigureAwait(false),
				SourceConfiguration = IsSyncProfile(config) ? CreateSourceConfiguration(workspaceID, _SAVED_SEARCH_ARTIFACT_ID) : string.Empty
			};
			return integrationPointProfile;
		}

		private static bool IsSyncProfile(ProfileConfig config) =>
			config.SourceProviderGuid == SourceProviders.RELATIVITY && config.DestinationProviderGuid == DestinationProviders.RELATIVITY;

		private static string CreateSourceConfiguration(int workspaceID, int savedSearchArtifactId)
		{
			var sourceConfiguration = new JObject
			{
				[nameof(kCura.IntegrationPoints.Services.RelativityProviderSourceConfiguration.SourceWorkspaceArtifactId)] = workspaceID,
				[nameof(kCura.IntegrationPoints.Services.RelativityProviderSourceConfiguration.SavedSearchArtifactId)] = savedSearchArtifactId
			};
			return sourceConfiguration.ToString();
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

		public static async Task<int> GetTypeArtifactIdAsync(int workspaceID, string typeName)
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

		private static IRelativityObjectManager CreateRelativityObjectManagerForWorkspace(int workspaceID) =>
			SystemTestsSetupFixture.Container.Resolve<IRelativityObjectManagerFactory>().CreateRelativityObjectManager(workspaceID);

		private class ProfileConfig
		{
			public string SourceProviderGuid { get; set; }
			public string DestinationProviderGuid { get; set; }
			public string TypeName { get; set; }
		}
	}
}