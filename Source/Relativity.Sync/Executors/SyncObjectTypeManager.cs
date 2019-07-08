using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class SyncObjectTypeManager : ISyncObjectTypeManager
	{
		private readonly IDestinationServiceFactoryForAdmin _serviceFactory;

		public SyncObjectTypeManager(IDestinationServiceFactoryForAdmin serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<int> EnsureObjectTypeExistsAsync(int workspaceArtifactId, Guid objectTypeGuid, ObjectTypeRequest objectTypeRequest)
		{
			using (IArtifactGuidManager artifactGuidManager = await _serviceFactory.CreateProxyAsync<IArtifactGuidManager>().ConfigureAwait(false))
			{
				bool guidExists = await artifactGuidManager.GuidExistsAsync(workspaceArtifactId, objectTypeGuid).ConfigureAwait(false);
				if (guidExists)
				{
					return await ReadObjectType(workspaceArtifactId, objectTypeGuid).ConfigureAwait(false);
				}
				else
				{
					int objectTypeArtifactId = await ReadOrCreateObjectTypeByNameAsync(workspaceArtifactId, objectTypeRequest).ConfigureAwait(false);
					await artifactGuidManager.CreateSingleAsync(workspaceArtifactId, objectTypeArtifactId, new List<Guid>() { objectTypeGuid }).ConfigureAwait(false);
					return objectTypeArtifactId;
				}
			}
		}

		private async Task<int> ReadOrCreateObjectTypeByNameAsync(int workspaceArtifactId, ObjectTypeRequest objectTypeRequest)
		{
			QueryResult queryByNameResult = await QueryObjectTypeByNameAsync(workspaceArtifactId, objectTypeRequest.Name).ConfigureAwait(false);
			if (queryByNameResult.Objects.Count > 0)
			{
				return queryByNameResult.Objects.First().ArtifactID;
			}
			else
			{
				using (IObjectTypeManager objectTypeManager = await _serviceFactory.CreateProxyAsync<IObjectTypeManager>().ConfigureAwait(false))
				{
					int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceArtifactId, objectTypeRequest).ConfigureAwait(false);

					return objectTypeArtifactId;
				}
			}
		}

		public async Task<QueryResult> QueryObjectTypeByNameAsync(int workspaceArtifactId, string name)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.ObjectType
					},
					Condition = $"'Name' == '{name}'"
				};
				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);
				return queryResult;
			}
		}

		private async Task<int> ReadObjectType(int workspaceArtifactId, Guid guid)
		{
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				ReadRequest request = new ReadRequest()
				{
					Object = new RelativityObjectRef()
					{
						Guid = guid
					}
				};
				ReadResult result = await objectManager.ReadAsync(workspaceArtifactId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}
	}
}