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
using RelativityProviderSourceConfiguration = kCura.IntegrationPoints.Services.RelativityProviderSourceConfiguration;

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
			List<int> createdProfilesArtifactIds = await CreateTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactId).ConfigureAwait(false);
			_teardownActions.AddLast(() => DeleteTestProfilesAsync(SystemTestsSetupFixture.SourceWorkspace.ArtifactId, createdProfilesArtifactIds).GetAwaiter().GetResult());

			// Act
			TestWorkspace createdWorkspace = await SystemTestsSetupFixture.CreateManagedWorkspaceWithDefaultName(SystemTestsSetupFixture.SourceWorkspace.Name)
				.ConfigureAwait(false);

			// Assert
			await VerifyAllProfilesInDestinationWorkspaceAreSyncOnlyAndHaveProperValuesSetAsync(createdWorkspace.ArtifactId)
				.ConfigureAwait(false);
		}

		[TearDown]
		public void TearDown()
		{
			SystemTestsSetupFixture.InvokeActionsAndResetFixtureOnException(_teardownActions);
		}

		private async Task VerifyAllProfilesInDestinationWorkspaceAreSyncOnlyAndHaveProperValuesSetAsync(int targetWorkspaceId)
		{
			var objectManager = CreateRelativityObjectManagerForWorkspace(targetWorkspaceId);

			// get all profiles from the created workspace
			var targetWorkspaceProfiles = await objectManager.QueryAsync<IntegrationPointProfile>(new QueryRequest())
					.ConfigureAwait(false);
			int expectedSyncProfilesCount = _testProfilesConfigs.Count(IsSyncProfile);
			targetWorkspaceProfiles.Should().HaveCount(expectedSyncProfilesCount);

			// NOTE: The assertions below should fail until completion of REL-351468

			// verify destination provider id
			int syncDestinationProviderArtifactId = await GetSyncDestinationProviderArtifactIdAsync(targetWorkspaceId)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.DestinationProvider)
				.ShouldAllBeEquivalentTo(syncDestinationProviderArtifactId);

			// verify source provider id
			int syncSourceProviderArtifactId = await GetSyncSourceProviderArtifactIdAsync(targetWorkspaceId)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.SourceProvider)
				.ShouldAllBeEquivalentTo(syncSourceProviderArtifactId);

			// verify integration point type id
			int exportIntegrationPointTypeArtifactId = await GetTypeArtifactIdAsync(targetWorkspaceId, IntegrationPointTypes.ExportName)
				.ConfigureAwait(false);
			targetWorkspaceProfiles.Select(p => p.Type)
				.ShouldBeEquivalentTo(exportIntegrationPointTypeArtifactId);

			var sourceConfigurations = targetWorkspaceProfiles.Select(p => p.SourceConfiguration)
				.Select(JObject.Parse)
				.ToList();
			// verify source workspace id in source configuration
			sourceConfigurations.Select(c => c[nameof(RelativityProviderSourceConfiguration.SourceWorkspaceArtifactId)])
				.Select(t => t.ToObject<int>())
				.ShouldAllBeEquivalentTo(targetWorkspaceId);

			// verify saved search id in source configuration
			sourceConfigurations.Select(c => c.Properties().FirstOrDefault(p => p.Name.Equals(nameof(RelativityProviderSourceConfiguration.SavedSearchArtifactId), StringComparison.OrdinalIgnoreCase)))
				.Should().OnlyContain(p => p == null || p.Value<int>() == 0);
		}

		private Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceId) =>
			GetSingleObjectArtifactIdByStringFieldValueAsync<DestinationProvider>(workspaceId,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

		private Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceId) =>
			GetSingleObjectArtifactIdByStringFieldValueAsync<SourceProvider>(workspaceId,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);

		private async Task<int> GetSingleObjectArtifactIdByStringFieldValueAsync<TSource>(int workspaceId,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			List<int> objectsArtifactIds = await _objectArtifactIdsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceId, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIds.Single();
			return artifactId;
		}

		private static async Task<List<int>> CreateTestProfilesAsync(int workspaceId)
		{
			var objectManager = CreateRelativityObjectManagerForWorkspace(workspaceId);

			IList<Task<IntegrationPointProfile>> profileCreationTasks = _testProfilesConfigs
				.Zip(Enumerable.Range(0, _testProfilesConfigs.Count()), (config, i) => new { Config = config, Number = i })
				.Select(x => CreateProfileRdoAsync(workspaceId, x.Number, x.Config))
				.ToList();

			await Task.WhenAll(profileCreationTasks).ConfigureAwait(false);

			var createdProfilesArtifactIds = profileCreationTasks
				.Select(t => t.Result)
				.Select(p => objectManager.Create(p))
				.ToList();

			return createdProfilesArtifactIds;
		}

		private static async Task<IntegrationPointProfile> CreateProfileRdoAsync(int workspaceId, int profileNumber, ProfileConfig config)
		{
			var integrationPointProfile = new IntegrationPointProfile
			{
				Name = $"Profile{profileNumber}",
				SourceProvider = await GetSourceProviderArtifactIdAsync(workspaceId, config.SourceProviderGuid).ConfigureAwait(false),
				DestinationProvider = await GetDestinationProviderArtifactIdAsync(workspaceId, config.DestinationProviderGuid).ConfigureAwait(false),
				Type = await GetTypeArtifactIdAsync(workspaceId, config.TypeName).ConfigureAwait(false),
				SourceConfiguration = IsSyncProfile(config) ? CreateSourceConfiguration(workspaceId, _SAVED_SEARCH_ARTIFACT_ID) : string.Empty
			};
			return integrationPointProfile;
		}

		private static bool IsSyncProfile(ProfileConfig config) =>
			config.SourceProviderGuid == SourceProviders.RELATIVITY && config.DestinationProviderGuid == DestinationProviders.RELATIVITY;

		private static string CreateSourceConfiguration(int workspaceId, int savedSearchArtifactId)
		{
			var sourceConfiguration = new JObject
			{
				[nameof(RelativityProviderSourceConfiguration.SourceWorkspaceArtifactId)] = workspaceId,
				[nameof(RelativityProviderSourceConfiguration.SavedSearchArtifactId)] = savedSearchArtifactId
			};
			return sourceConfiguration.ToString();
		}

		private static async Task DeleteTestProfilesAsync(int workspaceId, IEnumerable<int> profilesArtifactIdsToDelete)
		{
			using (var objectManager = SystemTestsSetupFixture.TestHelper.CreateProxy<IObjectManager>())
			{
				var request = new MassDeleteByObjectIdentifiersRequest
				{
					Objects = profilesArtifactIdsToDelete
						.Select(aid => new RelativityObjectRef { ArtifactID = aid })
						.ToList()
				};
				await objectManager.DeleteAsync(workspaceId, request).ConfigureAwait(false);
			}
		}

		public static async Task<int> GetTypeArtifactIdAsync(int workspaceId, string typeName)
		{
			using (var typeClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IIntegrationPointTypeManager>())
			{
				IList<IntegrationPointTypeModel> integrationPointTypes = await typeClient.GetIntegrationPointTypes(workspaceId).ConfigureAwait(false);

				IntegrationPointTypeModel foundIntegrationPointType = integrationPointTypes.FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
				if (foundIntegrationPointType == null)
				{
					throw new TestException($"Could not find {nameof(IntegrationPointTypeModel)} of name {typeName} in workspace {workspaceId}");
				}
				return foundIntegrationPointType.ArtifactId;
			}
		}

		private static async Task<int> GetSourceProviderArtifactIdAsync(int workspaceId, string guid)
		{
			using (var providerClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IProviderManager>())
			{
				return await providerClient.GetSourceProviderArtifactIdAsync(workspaceId, guid).ConfigureAwait(false);
			}
		}

		private static async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceId, string guid)
		{
			using (var providerClient = SystemTestsSetupFixture.TestHelper.CreateProxy<IProviderManager>())
			{
				return await providerClient.GetDestinationProviderArtifactIdAsync(workspaceId, guid).ConfigureAwait(false);
			}
		}

		private static IRelativityObjectManager CreateRelativityObjectManagerForWorkspace(int workspaceId) =>
			SystemTestsSetupFixture.Container.Resolve<IRelativityObjectManagerFactory>().CreateRelativityObjectManager(workspaceId);

		private class ProfileConfig
		{
			public string SourceProviderGuid { get; set; }
			public string DestinationProviderGuid { get; set; }
			public string TypeName { get; set; }
		}
	}
}