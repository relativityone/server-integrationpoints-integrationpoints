using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	internal class IntegrationPointProfilesQuery : IIntegrationPointProfilesQuery
	{
		private readonly Func<int, IRelativityObjectManager> _createRelativityObjectManager;
		private readonly IObjectArtifactIdsByStringFieldValueQuery _objectArtifactIdsByStringFieldValueQuery;

		public IntegrationPointProfilesQuery(Func<int, IRelativityObjectManager> createRelativityObjectManager, IObjectArtifactIdsByStringFieldValueQuery objectArtifactIdsByStringFieldValueQuery)
		{
			_createRelativityObjectManager = createRelativityObjectManager;
			_objectArtifactIdsByStringFieldValueQuery = objectArtifactIdsByStringFieldValueQuery;
		}

		public async Task<(List<int> nonSyncProfilesArtifactIds, List<int> syncProfilesArtifactIds)> GetSyncAndNonSyncProfilesArtifactIdsAsync(int workspaceId)
		{
			Task<int> syncSourceProviderArtifactIdTask = GetSyncSourceProviderArtifactIdAsync(workspaceId);
			Task<int> syncDestinationProviderArtifactIdTask = GetSyncDestinationProviderArtifactIdAsync(workspaceId);
			Task<List<IntegrationPointProfile>> getProfilesWithProvidersFromTemplateWorkspaceTask = GetProfilesWithProvidersFromWorkspaceAsync(workspaceId);
			await Task.WhenAll(
					syncSourceProviderArtifactIdTask,
					syncDestinationProviderArtifactIdTask,
					getProfilesWithProvidersFromTemplateWorkspaceTask)
				.ConfigureAwait(false);

			bool IsSyncProfile(IntegrationPointProfile integrationPointProfile) =>
				integrationPointProfile.DestinationProvider == syncDestinationProviderArtifactIdTask.Result &&
				integrationPointProfile.SourceProvider == syncSourceProviderArtifactIdTask.Result;

			List<IntegrationPointProfile> allProfiles = getProfilesWithProvidersFromTemplateWorkspaceTask.Result;

			List<int> nonSyncProfilesArtifactIds = allProfiles
				.Where(p => !IsSyncProfile(p))
				.Select(p => p.ArtifactId)
				.ToList();
			List<int> syncProfilesArtifactIds = allProfiles
				.Where(p => IsSyncProfile(p))
				.Select(p => p.ArtifactId)
				.ToList();
			return (nonSyncProfilesArtifactIds, syncProfilesArtifactIds);
		}

		private Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceId) =>
			GetSingleObjectArtifactIdByStringFieldValueAsync<DestinationProvider>(workspaceId,
				destinationProvider => destinationProvider.Identifier,
				Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

		private Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceId) =>
			GetSingleObjectArtifactIdByStringFieldValueAsync<SourceProvider>(workspaceId,
				sourceProvider => sourceProvider.Identifier,
				Constants.IntegrationPoints.SourceProviders.RELATIVITY);

		private async Task<int> GetSingleObjectArtifactIdByStringFieldValueAsync<TSource>(int workspaceId,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			List<int> objectsArtifactIds = await _objectArtifactIdsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceId, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIds.Single();
			return artifactId;
		}

		private async Task<List<IntegrationPointProfile>> GetProfilesWithProvidersFromWorkspaceAsync(int workspaceId)
		{
			var queryRequest = new QueryRequest
			{
				Fields = new[]
				{
					new FieldRef
					{
						Guid = IntegrationPointProfileFieldGuids.DestinationProviderGuid
					},
					new FieldRef
					{
						Guid = IntegrationPointProfileFieldGuids.SourceProviderGuid
					}
				}
			};
			List<IntegrationPointProfile> integrationPointProfiles = await _createRelativityObjectManager(workspaceId)
				.QueryAsync<IntegrationPointProfile>(queryRequest)
				.ConfigureAwait(false);

			return integrationPointProfiles;
		}
	}
}
