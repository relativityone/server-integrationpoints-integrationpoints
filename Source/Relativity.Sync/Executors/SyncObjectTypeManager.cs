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
		private readonly ISyncLog _logger;

		public SyncObjectTypeManager(IDestinationServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<int> EnsureObjectTypeExistsAsync(int workspaceArtifactId, Guid objectTypeGuid, ObjectTypeRequest objectTypeRequest)
		{
			using (IArtifactGuidManager artifactGuidManager = await _serviceFactory.CreateProxyAsync<IArtifactGuidManager>().ConfigureAwait(false))
			{
				bool guidExists = await artifactGuidManager.GuidExistsAsync(workspaceArtifactId, objectTypeGuid).ConfigureAwait(false);
				if (guidExists)
				{
					_logger.LogVerbose("Object type name '{objectTypeName}' with GUID: {objectTypeGuid} already exists.", objectTypeRequest.Name, objectTypeGuid);
					return await ReadObjectType(workspaceArtifactId, objectTypeGuid).ConfigureAwait(false);
				}
				else
				{
					int objectTypeArtifactId = await ReadOrCreateObjectTypeByNameAsync(workspaceArtifactId, objectTypeRequest).ConfigureAwait(false);
					_logger.LogVerbose("Assigning GUID {objectTypeGuid} to object type name '{objectTypeName}' with Artifact ID: {objectTypeArtifactId}",
						objectTypeGuid, objectTypeRequest.Name, objectTypeArtifactId);
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
				_logger.LogVerbose("Object type name '{objectTypeName}' already exists, but may not have GUID assigned.", objectTypeRequest.Name);
				return queryByNameResult.Objects.First().ArtifactID;
			}
			else
			{
				_logger.LogVerbose("Creating object type name '{objectTypeName}'", objectTypeRequest.Name);
				using (IObjectTypeManager objectTypeManager = await _serviceFactory.CreateProxyAsync<IObjectTypeManager>().ConfigureAwait(false))
				{
					int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceArtifactId, objectTypeRequest).ConfigureAwait(false);

					return objectTypeArtifactId;
				}
			}
		}

		public async Task<QueryResult> QueryObjectTypeByNameAsync(int workspaceArtifactId, string name)
		{
			_logger.LogVerbose("Querying for object type name '{name}'", name);
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
			_logger.LogVerbose("Reading artifact ID of object type with GUID: {guid}", guid);
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