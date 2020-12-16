using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class PermissionsCheckConfiguration : IPermissionsCheckConfiguration
	{
		private readonly Lazy<int> _sourceArtifactId = null;

		private readonly IConfiguration _cache;
		private readonly ISourceServiceFactoryForUser _sourceServiceFactory;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid SourceProviderGuid = new Guid("5be4a1f7-87a8-4cbe-a53f-5027d4f70b80");
		private static readonly Guid RelativityProviderGuid = new Guid("423b4d43-eae9-4e14-b767-17d629de4bb2");

		public PermissionsCheckConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISourceServiceFactoryForUser sourceServiceFactory)
		{
			_cache = cache;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
			_sourceServiceFactory = sourceServiceFactory;
			_sourceArtifactId = new Lazy<int>(() => GetSourceProviderArtifactIdAsync().ConfigureAwait(false).GetAwaiter()
				.GetResult());
		}

		public int SourceWorkspaceArtifactId { get; }

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);

		public int DestinationFolderArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DataDestinationArtifactIdGuid);

		public int SourceProviderArtifactId => _sourceArtifactId.Value;

		public bool CreateSavedSearchForTags => _cache.GetFieldValue<bool>(SyncConfigurationRdo.CreateSavedSearchInDestinationGuid);

		private async Task<int> GetSourceProviderArtifactIdAsync()
		{
			using (var objectManager = await _sourceServiceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						Guid = SourceProviderGuid
					},
					Condition = $"'Identifier' == '{RelativityProviderGuid}'"
				};
				QueryResult objectManagerValue = await objectManager.QueryAsync(SourceWorkspaceArtifactId, queryRequest, 0, 1)
					.ConfigureAwait(false);

				const int countOfObjects = 1;
				if (objectManagerValue.TotalCount == countOfObjects)
				{
					return objectManagerValue.Objects.First().ArtifactID;
				}
				throw new SyncException($"Error while querying for 'Relativity' provider using ObjectManager. Query returned {objectManagerValue.TotalCount} objects, but exactly {countOfObjects} was expected.");
			}
		}
	}
}