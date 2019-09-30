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

		public async Task<IEnumerable<IntegrationPointProfile>> GetAllProfilesAsync(int workspaceID)
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
			IList<IntegrationPointProfile> integrationPointProfiles = await _createRelativityObjectManager(workspaceID)
				.QueryAsync<IntegrationPointProfile>(queryRequest)
				.ConfigureAwait(false);

			return integrationPointProfiles;
		}

		public Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<DestinationProvider>(workspaceID,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);
		}

		public Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<SourceProvider>(workspaceID,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);
		}

		public Task<IEnumerable<int>> GetSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			IEnumerable<int> nonSyncProfiles = FilterProfiles(profiles, (profile) => IsSyncProfile(profile, syncSourceProviderArtifactID, syncDestinationProviderArtifactID));
			return Task.FromResult(nonSyncProfiles);
		}

		public Task<IEnumerable<int>> GetNonSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			IEnumerable<int> nonSyncProfiles = FilterProfiles(profiles, (profile) => !IsSyncProfile(profile, syncSourceProviderArtifactID, syncDestinationProviderArtifactID));
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

		private bool IsSyncProfile(IntegrationPointProfile integrationPointProfile, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			return integrationPointProfile.DestinationProvider == syncDestinationProviderArtifactID &&
			       integrationPointProfile.SourceProvider == syncSourceProviderArtifactID;
		}

		private async Task<int> GetSingleObjectArtifactIdByStringFieldValueAsync<TSource>(int workspaceID,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			IEnumerable<int> objectsArtifactIDs = await _objectArtifactIdsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceID, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIDs.Single();
			return artifactId;
		}

	}
}
