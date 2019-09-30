using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

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

		public async Task<IEnumerable<IntegrationPointProfile>> GetAllProfilesAsync(int workspaceId)
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
			IList<IntegrationPointProfile> integrationPointProfiles = await _createRelativityObjectManager(workspaceId)
				.QueryAsync<IntegrationPointProfile>(queryRequest)
				.ConfigureAwait(false);

			return integrationPointProfiles;
		}

		public Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceId)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<DestinationProvider>(workspaceId,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);
		}

		public Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceId)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<SourceProvider>(workspaceId,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);
		}

		public Task<IEnumerable<int>> GetSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactId, int syncDestinationProviderArtifactId)
		{
			IEnumerable<int> nonSyncProfiles = FilterProfiles(profiles, (profile) => IsSyncProfile(profile, syncSourceProviderArtifactId, syncDestinationProviderArtifactId));
			return Task.FromResult(nonSyncProfiles);
		}

		public Task<IEnumerable<int>> GetNonSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactId, int syncDestinationProviderArtifactId)
		{
			IEnumerable<int> nonSyncProfiles = FilterProfiles(profiles, (profile) => !IsSyncProfile(profile, syncSourceProviderArtifactId, syncDestinationProviderArtifactId));
			return Task.FromResult(nonSyncProfiles);
		}

		private List<int> FilterProfiles(IEnumerable<IntegrationPointProfile> profiles, Func<IntegrationPointProfile, bool> filter)
		{
			List<int> filteredProfiles = profiles
				.Where(filter)
				.Select(profile => profile.ArtifactId)
				.ToList();
			return filteredProfiles;
		}

		private bool IsSyncProfile(IntegrationPointProfile integrationPointProfile, int syncSourceProviderArtifactId, int syncDestinationProviderArtifactId)
		{
			return integrationPointProfile.DestinationProvider == syncDestinationProviderArtifactId &&
				   integrationPointProfile.SourceProvider == syncSourceProviderArtifactId;
		}

		private async Task<int> GetSingleObjectArtifactIdByStringFieldValueAsync<TSource>(int workspaceId,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			IEnumerable<int> objectsArtifactIds = await _objectArtifactIdsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceId, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIds.Single();
			return artifactId;
		}

	}
}
