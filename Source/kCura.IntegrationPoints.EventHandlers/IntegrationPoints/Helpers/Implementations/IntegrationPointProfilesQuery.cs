﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	internal class IntegrationPointProfilesQuery : IIntegrationPointProfilesQuery
	{
		private readonly Func<int, IRelativityObjectManager> _createRelativityObjectManager;
		private readonly IObjectArtifactIdsByStringFieldValueQuery _objectArtifactIDsByStringFieldValueQuery;
		private readonly ISerializer _serializer;

		public IntegrationPointProfilesQuery(Func<int, IRelativityObjectManager> createRelativityObjectManager, IObjectArtifactIdsByStringFieldValueQuery objectArtifactIDsByStringFieldValueQuery,
			ISerializer serializer)
		{
			_createRelativityObjectManager = createRelativityObjectManager;
			_objectArtifactIDsByStringFieldValueQuery = objectArtifactIDsByStringFieldValueQuery;
			_serializer = serializer;
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
						Guid = IntegrationPointProfileFieldGuids.DestinationConfigurationGuid
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

		public IEnumerable<IntegrationPointProfile> GetProfilesToUpdate(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			IEnumerable<IntegrationPointProfile> profilesToUpdate = FilterProfiles(profiles, (profile) =>
				CanProfileBePreservedAndUpdated(profile, syncSourceProviderArtifactID, syncDestinationProviderArtifactID));
			return profilesToUpdate;
		}

		public IEnumerable<IntegrationPointProfile> GetProfilesToDelete(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			IEnumerable<IntegrationPointProfile> profilesToDelete = FilterProfiles(profiles, (profile) =>
				!CanProfileBePreservedAndUpdated(profile, syncSourceProviderArtifactID, syncDestinationProviderArtifactID));
			return profilesToDelete;
		}

		private IEnumerable<IntegrationPointProfile> FilterProfiles(IEnumerable<IntegrationPointProfile> profiles, Func<IntegrationPointProfile, bool> filter)
		{
			IEnumerable<IntegrationPointProfile> filteredProfiles = profiles
				.Where(filter);
			return filteredProfiles;
		}

		private bool CanProfileBePreservedAndUpdated(IntegrationPointProfile integrationPointProfile, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID)
		{
			SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(integrationPointProfile.SourceConfiguration);
			bool isProductionAsSource = sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.ProductionSet;
			
			ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationPointProfile.DestinationConfiguration);
			bool isProductionAsDestination = destinationConfiguration.ProductionImport;

			bool isProductionSelectedAsSourceOrDestination = isProductionAsSource || isProductionAsDestination;

			return integrationPointProfile.DestinationProvider == syncDestinationProviderArtifactID &&
			       integrationPointProfile.SourceProvider == syncSourceProviderArtifactID &&
			       !isProductionSelectedAsSourceOrDestination;
		}

		public Task<int> GetSyncDestinationProviderArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIDByStringFieldValueAsync<DestinationProvider>(workspaceID,
				destinationProvider => destinationProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY);
		}

		public Task<int> GetSyncSourceProviderArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIDByStringFieldValueAsync<SourceProvider>(workspaceID,
				sourceProvider => sourceProvider.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY);
		}

		public Task<int> GetIntegrationPointExportTypeArtifactIDAsync(int workspaceID)
		{
			return GetSingleObjectArtifactIDByStringFieldValueAsync<IntegrationPointType>(workspaceID,
				type => type.Identifier,
				kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString());
		}

		private async Task<int> GetSingleObjectArtifactIDByStringFieldValueAsync<TSource>(int workspaceID,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			IEnumerable<int> objectsArtifactIDs = await _objectArtifactIDsByStringFieldValueQuery
				.QueryForObjectArtifactIdsByStringFieldValueAsync(workspaceID, propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactID = objectsArtifactIDs.Single();
			return artifactID;
		}

	}
}
