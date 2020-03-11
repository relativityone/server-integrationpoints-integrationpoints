﻿using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Helpers;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldsRepository : IFieldsRepository
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		private readonly IServicesMgr _servicesMgr;

		public FieldsRepository(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public async Task<IEnumerable<DocumentFieldInfo>> GetAllDocumentFieldsAsync(int workspaceID)
		{
			QueryRequest queryRequest = PrepareFieldsQueryRequest($"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID}");
			IEnumerable<RelativityObject> fieldObjects = await GetFieldsByQueryAsync(workspaceID, queryRequest).ConfigureAwait(false);

			return fieldObjects.Select(f => FieldConvert.ToDocumentFieldInfo(f));
		}

		public async Task<IEnumerable<DocumentFieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIDs, int workspaceID)
		{
			if(artifactIDs == null || !artifactIDs.Any())
			{
				return Enumerable.Empty<DocumentFieldInfo>();
			}

			QueryRequest queryRequest = PrepareFieldsQueryRequest($"'ArtifactID' IN [{string.Join(",", artifactIDs)}]");
			IEnumerable<RelativityObject> fieldObjects = await GetFieldsByQueryAsync(workspaceID, queryRequest).ConfigureAwait(false);

			return fieldObjects.Select(f => FieldConvert.ToDocumentFieldInfo(f));
		}

		private QueryRequest PrepareFieldsQueryRequest(string query)
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = query,
				Fields = new[]
				{
						new FieldRef()
						{
							Name = "*"
						}
					},
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}

		private async Task<IEnumerable<RelativityObject>> GetFieldsByQueryAsync(int workspaceID, QueryRequest queryRequest)
		{
			using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
			{
				const int queryBatchSize = 50;
				int resultCount = 0;
				List<RelativityObject> retrievedObjects = new List<RelativityObject>();

				do
				{
					QueryResult queryResult = await objectManager.QueryAsync(workspaceID, queryRequest,
						start: retrievedObjects.Count + 1, length: queryBatchSize).ConfigureAwait(false);
					retrievedObjects.AddRange(queryResult.Objects);
					resultCount = queryResult.ResultCount;
				} while (resultCount > 0);

				return retrievedObjects;
			}
		}
	}
}
