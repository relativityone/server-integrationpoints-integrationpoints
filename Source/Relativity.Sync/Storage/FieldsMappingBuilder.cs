using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Storage
{
	internal class FieldsMappingBuilder : IFieldsMappingBuilder
	{
		private readonly int _sourceWorkspaceId;
		private readonly int _destinationWorkspaceId;

		private readonly ISyncServiceManager _servicesMgr;

		public FieldsMappingBuilder(int sourceWorkspaceId, int destinationWorkspaceId, ISyncServiceManager servicesMgr)
		{
			_sourceWorkspaceId = sourceWorkspaceId;
			_destinationWorkspaceId = destinationWorkspaceId;

			_servicesMgr = servicesMgr;

			FieldsMapping = new List<FieldMap>();
		}

		public List<FieldMap> FieldsMapping { get; }
		
		public IFieldsMappingBuilder WithIdentifier()
		{
			if (FieldsMapping.Exists(x => x.FieldMapType == FieldMapType.Identifier))
			{
				throw new InvalidOperationException("Identifier has been mapped already");
			}

			using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				QueryRequest query = PrepareIdentifierFieldsQueryRequest();

				RelativityObject sourceField = objectManager.QueryAsync(_sourceWorkspaceId, query, 0, 1)
					.GetAwaiter().GetResult().Objects.Single();
				RelativityObject destinationField = objectManager.QueryAsync(_destinationWorkspaceId, query, 0, 1)
					.GetAwaiter().GetResult().Objects.Single();

				FieldsMapping.Add(new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = sourceField.Name,
						FieldIdentifier = sourceField.ArtifactID,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry
					{
						DisplayName = destinationField.Name,
						FieldIdentifier = destinationField.ArtifactID,
						IsIdentifier = true
					},
					FieldMapType = FieldMapType.Identifier
				});

				return this;
			}
		}

		public IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId)
		{
			using (var fieldManager = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				var sourceField = fieldManager.ReadAsync(_sourceWorkspaceId, sourceFieldId).GetAwaiter().GetResult();
				var destinationField = fieldManager.ReadAsync(_destinationWorkspaceId, sourceFieldId).GetAwaiter().GetResult();

				if (sourceField.IsIdentifier || destinationField.IsIdentifier)
				{
					throw new ArgumentException("One of the specified fields is identifier. " +
						$"Source Field Id - {sourceFieldId}, Destination Field Id - {destinationFieldId}");
				}

				FieldsMapping.Add(new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = sourceField.Name,
						FieldIdentifier = sourceField.ArtifactID,
						IsIdentifier = false
					},
					DestinationField = new FieldEntry
					{
						DisplayName = destinationField.Name,
						FieldIdentifier = destinationField.ArtifactID,
						IsIdentifier = false
					},
					FieldMapType = FieldMapType.None

				});

				return this;
			}
		}

		private QueryRequest PrepareIdentifierFieldsQueryRequest()
		{
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = (int)ArtifactType.Field
				},
				Condition = "'Is Identifier' == true",
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}
	}
}
