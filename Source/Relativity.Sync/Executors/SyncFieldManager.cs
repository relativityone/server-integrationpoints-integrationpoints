using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class SyncFieldManager : ISyncFieldManager
	{
		private readonly IDestinationServiceFactoryForAdmin _serviceFactory;
		private readonly ISyncLog _logger;

		public SyncFieldManager(IDestinationServiceFactoryForAdmin serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task EnsureFieldsExistAsync(int workspaceArtifactId, IDictionary<Guid, BaseFieldRequest> fieldRequests)
		{
			if (fieldRequests == null)
			{
				return;
			}

			using (IArtifactGuidManager artifactGuidManager = await _serviceFactory.CreateProxyAsync<IArtifactGuidManager>().ConfigureAwait(false))
			{
				for (int i = 0; i < fieldRequests.Count; i++)
				{
					Guid fieldGuid = fieldRequests.Keys.ElementAt(i);
					BaseFieldRequest fieldRequest = fieldRequests.Values.ElementAt(i);

					bool guidExists = await artifactGuidManager.GuidExistsAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);
					_logger.LogVerbose("Field name '{fieldName}' with GUID: {guid} already exists.", fieldRequest.Name, fieldGuid);
					if (!guidExists)
					{
						int fieldArtifactId = await ReadOrCreateFieldAsync(workspaceArtifactId, fieldRequest).ConfigureAwait(false);
						_logger.LogVerbose("Assigning GUID {fieldGuid} to field name '{fieldName}' with Artifact ID: {fieldArtifactId}",
							fieldGuid, fieldRequest.Name, fieldArtifactId);
						await artifactGuidManager.CreateSingleAsync(workspaceArtifactId, fieldArtifactId, new List<Guid>() { fieldGuid }).ConfigureAwait(false);
					}
				}
			}
		}

		private async Task<int> ReadOrCreateFieldAsync(int workspaceArtifactId, BaseFieldRequest fieldRequest)
		{
			QueryResult queryByNameResult = await QueryFieldByNameAsync(workspaceArtifactId, fieldRequest.Name).ConfigureAwait(false);
			if (queryByNameResult.Objects.Count > 0)
			{
				_logger.LogVerbose("Field name '{fieldName}' already exists, but may not have GUID assigned.", fieldRequest.Name);
				return queryByNameResult.Objects.First().ArtifactID;
			}
			else
			{
				_logger.LogVerbose("Creating field name '{fieldName}'", fieldRequest.Name);
				using (Services.Interfaces.Field.IFieldManager fieldManager = await _serviceFactory.CreateProxyAsync<Services.Interfaces.Field.IFieldManager>().ConfigureAwait(false))
				{
					if (fieldRequest is WholeNumberFieldRequest wholeNumberFieldRequest)
					{
						return await fieldManager.CreateWholeNumberFieldAsync(workspaceArtifactId, wholeNumberFieldRequest).ConfigureAwait(false);
					}
					else if (fieldRequest is FixedLengthFieldRequest fixedLengthFieldRequest)
					{
						return await fieldManager.CreateFixedLengthFieldAsync(workspaceArtifactId, fixedLengthFieldRequest).ConfigureAwait(false);
					}
					else if (fieldRequest is MultipleObjectFieldRequest multipleObjectFieldRequest)
					{
						return await fieldManager.CreateMultipleObjectFieldAsync(workspaceArtifactId, multipleObjectFieldRequest).ConfigureAwait(false);
					}
					else
					{
						string typeName = fieldRequest.GetType().ToString();
						_logger.LogError("Sync doesn't support creation of field type: {fieldType}", typeName);
						throw new NotSupportedException($"Sync doesn't support creation of field type: {typeName}");
					}
				}
			}
		}

		private async Task<QueryResult> QueryFieldByNameAsync(int workspaceArtifactId, string fieldName)
		{
			_logger.LogVerbose("Querying for field name '{fieldName}'", fieldName);
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.Field
					},
					Condition = $"'Name' == '{fieldName}'"
				};
				QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);
				return queryResult;
			}
		}
	}
}