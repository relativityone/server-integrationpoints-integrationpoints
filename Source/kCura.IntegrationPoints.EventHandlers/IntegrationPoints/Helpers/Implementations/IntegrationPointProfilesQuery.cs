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
					new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid

					},
					new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.SourceProviderGuid

					},
					new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.DestinationProviderGuid

					},
					new FieldRef()
					{
						Guid = IntegrationPointProfileFieldGuids.TypeGuid
					}
				}
			};
			IList<IntegrationPointProfile> integrationPointProfiles = await _createRelativityObjectManager(workspaceID)
				.QueryAsync<IntegrationPointProfile>(queryRequest)
				.ConfigureAwait(false);

			return integrationPointProfiles;
		}

		public Task<IEnumerable<IntegrationPointProfile>> GetSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			IEnumerable<IntegrationPointProfile> nonSyncProfiles = FilterProfiles(profiles, (profile) => IsSyncProfile(profile, syncSourceProviderArtifactID, syncDestinationProviderArtifactID));
			return Task.FromResult(nonSyncProfiles);
		}

		public Task<IEnumerable<IntegrationPointProfile>> GetNonSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			IEnumerable<IntegrationPointProfile> nonSyncProfiles = FilterProfiles(profiles, (profile) => !IsSyncProfile(profile, syncSourceProviderArtifactID, syncDestinationProviderArtifactID));
			return Task.FromResult(nonSyncProfiles);
		}

		private List<IntegrationPointProfile> FilterProfiles(IEnumerable<IntegrationPointProfile> profiles, Func<IntegrationPointProfile, bool> filter)
		{
			List<IntegrationPointProfile> filteredProfiles = profiles
				.Where(filter)
				.ToList();
			return filteredProfiles;
		}

		private bool IsSyncProfile(IntegrationPointProfile integrationPointProfile, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			return integrationPointProfile.DestinationProvider == syncDestinationProviderArtifactID &&
				   integrationPointProfile.SourceProvider == syncSourceProviderArtifactID;
		}

		public Task<int> GetSyncDestinationProviderArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<DestinationProvider>(workspaceID,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);
		}

		public Task<int> GetSyncSourceProviderArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<SourceProvider>(workspaceID,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);
		}

		public Task<int> GetIntegrationPointExportTypeArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIdByStringFieldValueAsync<IntegrationPointType>(workspaceID,
				type => type.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());
		}

		private async Task<int> GetSingleObjectArtifactIdByStringFieldValueAsync<TSource>(int workspaceID,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			IEnumerable<int> objectsArtifactIDs = await _objectArtifactIdsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceID, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactID = objectsArtifactIDs.Single();
			return artifactID;
		}

	}
}
