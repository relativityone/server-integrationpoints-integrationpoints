using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class DocumentFieldRepository : IDocumentFieldRepository
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public DocumentFieldRepository(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<IDictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsByFieldNameAsync(int sourceWorkspaceArtifactId, ICollection<string> fieldNames,
			CancellationToken token)
		{
			if (fieldNames.Count == 0)
			{
				throw new ArgumentException("Field names list is empty.", nameof(fieldNames));
			}

			string concatenatedFieldNames = string.Join(", ", fieldNames.Select(f => $"'{f}'"));
			QueryRequest request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef {Name = "Field"},
				Condition = $"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
				Fields = new[] { new FieldRef {Name = "Name"}, new FieldRef {Name = "Field Type"}}
			};

			QueryResultSlim result;
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					const int start = 0;
					result = await objectManager.QuerySlimAsync(sourceWorkspaceArtifactId, request, start, fieldNames.Count, token).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, "Service call failed while querying document field object: {request}", request);
					throw new SyncKeplerException($"Service call failed while querying document fields in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to query document field object: {request}", request);
					throw new SyncKeplerException($"Failed to query document fields in workspace {sourceWorkspaceArtifactId}", ex);
				}
			}

			Dictionary<string, RelativityDataType> parsedResult = result.Objects.ToDictionary(obj => (string) obj.Values[0], obj => ((string) obj.Values[1]).ToRelativityDataType());
			return parsedResult;
		}
	}
}
