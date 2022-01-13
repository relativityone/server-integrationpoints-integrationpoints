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
	internal sealed class ObjectFieldTypeRepository : IObjectFieldTypeRepository
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public ObjectFieldTypeRepository(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<IDictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsByFieldNameAsync(
			int sourceWorkspaceArtifactId, int sourceRdoArtifactId, ICollection<string> fieldNames,
			CancellationToken token)
		{
			if (fieldNames == null || fieldNames.Count == 0)
			{
				return new Dictionary<string, RelativityDataType>();
			}

			ICollection<string> requestedFieldNames = new HashSet<string>(fieldNames);

			IEnumerable<string> formattedFieldNames = requestedFieldNames.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(f => $"'{f}'");
			string concatenatedFieldNames = string.Join(", ", formattedFieldNames);
			QueryRequest request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { Name = "Field" },
				Condition = $"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {sourceRdoArtifactId}",
				Fields = new[]
				{
					new FieldRef { Name = "Name" },
					new FieldRef { Name = "Field Type" }
				}
			};

			QueryResultSlim result;
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					const int start = 0;
					result = await objectManager.QuerySlimAsync(sourceWorkspaceArtifactId, request, start, requestedFieldNames.Count, token).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, "Service call failed while querying document fields in workspace {workspaceArtifactId}", sourceWorkspaceArtifactId);
					throw new SyncKeplerException($"Service call failed while querying document fields in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to query document fields in workspace {workspaceArtifactId}", sourceWorkspaceArtifactId);
					throw new SyncKeplerException($"Failed to query document fields in workspace {sourceWorkspaceArtifactId}", ex);
				}
			}

			IEnumerable<RelativityObjectSlim> returnedObjects = result.Objects ?? Enumerable.Empty<RelativityObjectSlim>();
			Dictionary<string, RelativityDataType> fieldNameToFieldType = returnedObjects.ToDictionary(GetFieldName, GetFieldType);

			ICollection<string> returnedFieldNames = fieldNameToFieldType.Keys;

			if (returnedFieldNames.Count < requestedFieldNames.Count)
			{
				IEnumerable<string> missingFieldNames = requestedFieldNames.Except(returnedFieldNames);
				throw new FieldNotFoundException(
					$"Could not get information on the following fields for artifact type {sourceRdoArtifactId} in workspace {sourceWorkspaceArtifactId}. " +
					$"Make sure that they are present in the workspace and that the user running the job has access to them: {string.Join(", ", missingFieldNames)}");
			}

			return fieldNameToFieldType;
		}

		private static string GetFieldName(RelativityObjectSlim obj) => obj.Values[0].ToString();

		private static RelativityDataType GetFieldType(RelativityObjectSlim obj)
		{
			string rawValue = obj.Values[1].ToString();
			return rawValue.GetEnumFromDescription<RelativityDataType>();
		}
	}
}
