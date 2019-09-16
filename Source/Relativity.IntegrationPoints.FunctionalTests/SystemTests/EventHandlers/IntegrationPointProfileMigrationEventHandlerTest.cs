using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.FunctionalTests.SystemTests.EventHandlers
{
	[TestFixture]
	[Ignore("The test shouldn't pass until completion of REL-351468 and should be unignored then.")]
	public class IntegrationPointProfileMigrationEventHandlerTest
	{
		private const int _SAVED_SEARCH_ARTIFACT_ID = 123456;
		private static readonly Guid _destinationProviderObjectTypeGuid = Guid.Parse("d014f00d-f2c0-4e7a-b335-84fcb6eae980");
		private static readonly Guid _identifierFieldOnDestinationProviderObjectGuid = Guid.Parse("9fa104ac-13ea-4868-b716-17d6d786c77a");
		private static readonly Guid _relativityDestinationProviderTypeGuid = Guid.Parse("74A863B9-00EC-4BB7-9B3E-1E22323010C6");
		private static readonly Guid _sourceProviderObjectTypeGuid = Guid.Parse("5BE4A1F7-87A8-4CBE-A53F-5027D4F70B80");
		private static readonly Guid _identifierFieldOnSourceProviderObjectGuid = Guid.Parse("d0ecc6c9-472c-4296-83e1-0906f0c0fbb9");
		private static readonly Guid _relativitySourceProviderTypeGuid = Guid.Parse("423b4d43-eae9-4e14-b767-17d629de4bb2");

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

		[Test]
		public async Task ItShouldCopyOnlySyncProfiles()
		{
			List<int> createdProfilesArtifactIds = await CreateTestProfilesAsync(SystemTestsSetupFixture.WorkspaceID).ConfigureAwait(false);
			_teardownActions.AddLast(() => DeleteTestProfilesAsync(SystemTestsSetupFixture.WorkspaceID, createdProfilesArtifactIds).GetAwaiter().GetResult());

			// Act
			int workspaceArtifactId = await Workspace.CreateWorkspaceAsync(
				$"Rip.SystemTests.ProfileMigration-{DateTime.Now.Millisecond}",
				SystemTestsSetupFixture.WorkspaceName)
				.ConfigureAwait(false);
			_teardownActions.AddLast(() => Workspace.DeleteWorkspace(workspaceArtifactId));

			// Assert
			await VerifyAllProfilesInDestinationWorkspaceAreSyncOnlyAndHaveProperValuesSetAsync(workspaceArtifactId).ConfigureAwait(false);
		}

		[TearDown]
		public void TearDown()
		{
			SystemTestsSetupFixture.InvokeActionsAndResetFixtureOnException(_teardownActions);
		}

		private static async Task VerifyAllProfilesInDestinationWorkspaceAreSyncOnlyAndHaveProperValuesSetAsync(int targetWorkspaceId)
		{
			var objectManager = CreateRelativityObjectManagerForWorkspace(targetWorkspaceId);

			// get all profiles from the created workspace
			var targetWorkspaceProfiles = await objectManager.QueryAsync<IntegrationPointProfile>(new QueryRequest())
					.ConfigureAwait(false);
			targetWorkspaceProfiles.Should().HaveCount(1);

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

		private static async Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceId) =>
			await GetObjectArtifactIdByGuidFieldValueAsync(workspaceId, _sourceProviderObjectTypeGuid, _identifierFieldOnSourceProviderObjectGuid, _relativitySourceProviderTypeGuid)
				.ConfigureAwait(false);

		private static async Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceId) =>
			await GetObjectArtifactIdByGuidFieldValueAsync(workspaceId, _destinationProviderObjectTypeGuid, _identifierFieldOnDestinationProviderObjectGuid, _relativityDestinationProviderTypeGuid)
				.ConfigureAwait(false);

		private static async Task<int> GetObjectArtifactIdByGuidFieldValueAsync(int workspaceId, Guid objectTypeGuid, Guid fieldGuid, Guid value)
		{
			using (var objectManager = SystemTestsSetupFixture.TestHelper.CreateProxy<IObjectManager>())
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
				QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 0, 1)
					.ConfigureAwait(false);

				if (queryResult.TotalCount < 1)
				{
					throw new TestException($"Relativity object type {objectTypeGuid} with field {fieldGuid} of value {value} in workspace of id {workspaceId} was not found");
				}

				int artifactId = queryResult.Objects.First().ArtifactID;
				return artifactId;
			}
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