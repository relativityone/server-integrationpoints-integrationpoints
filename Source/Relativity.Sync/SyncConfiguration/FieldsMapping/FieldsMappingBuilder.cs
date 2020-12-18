﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration.FieldsMapping
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
				throw InvalidFieldsMappingException.IdentifierMappedTwice();
			}

			using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var sourceField = ReadIdentifierFieldAsync(_sourceWorkspaceId, objectManager).GetAwaiter().GetResult();
				var destinationField = ReadIdentifierFieldAsync(_destinationWorkspaceId, objectManager).GetAwaiter().GetResult();

				FieldsMapping.Add(new FieldMap
				{
					SourceField = sourceField,
					DestinationField = destinationField,
					FieldMapType = FieldMapType.Identifier
				});

				return this;
			}
		}

		public IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId)
		{
			using (var fieldManager = _servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
			{
				var sourceField = ReadFieldEntryByIdAsync(_sourceWorkspaceId, sourceFieldId, fieldManager).GetAwaiter().GetResult();
				var destinationField = ReadFieldEntryByIdAsync(_destinationWorkspaceId, destinationFieldId, fieldManager).GetAwaiter().GetResult();

				FieldsMapping.Add(new FieldMap
				{
					SourceField = sourceField,
					DestinationField = destinationField,
					FieldMapType = FieldMapType.None
				});

				return this;
			}
		}

		public IFieldsMappingBuilder WithField(string sourceFieldName, string destinationFieldName)
		{
			using (var objectManager = _servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var sourceField = ReadFieldEntryByNameAsync(_sourceWorkspaceId, sourceFieldName, objectManager).GetAwaiter().GetResult();
				var destinationField = ReadFieldEntryByNameAsync(_destinationWorkspaceId, destinationFieldName, objectManager).GetAwaiter().GetResult();

				FieldsMapping.Add(new FieldMap
				{
					SourceField = sourceField,
					DestinationField = destinationField,
					FieldMapType = FieldMapType.None
				});

				return this;
			}
		}

		private async Task<FieldEntry> ReadIdentifierFieldAsync(int workspaceId, IObjectManager objectManager)
		{
			QueryRequest request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = (int)ArtifactType.Field
				},
				Condition = "'Is Identifier' == true",
				IncludeNameInQueryResult = true
			};

			QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

			if (result.ResultCount == 0)
			{
				throw new InvalidFieldsMappingException("Identifier field not found");
			}

			RelativityObject field = result.Objects.Single();

			return new FieldEntry
			{
				DisplayName = field.Name,
				FieldIdentifier = field.ArtifactID,
				IsIdentifier = true
			};
		}

		private async Task<FieldEntry> ReadFieldEntryByIdAsync(int workspaceId, int fieldId, IFieldManager fieldManager)
		{
			var field = await fieldManager.ReadAsync(workspaceId, fieldId).ConfigureAwait(false);
			
			if (field == null)
			{
				throw InvalidFieldsMappingException.FieldNotFound(fieldId);
			}

			var fieldEntry = new FieldEntry
			{
				DisplayName = field.Name,
				FieldIdentifier = field.ArtifactID,
				IsIdentifier = field.IsIdentifier
			};

			if (fieldEntry.IsIdentifier)
			{
				throw InvalidFieldsMappingException.FieldIsIdentifier(fieldEntry.FieldIdentifier);
			}

			return fieldEntry;
		}

		private async Task<FieldEntry> ReadFieldEntryByNameAsync(int workspaceId, string fieldName, IObjectManager objectManager)
		{
			QueryRequest request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = (int)ArtifactType.Field
				},
				Condition = $"'Name' == '{fieldName}'",
				Fields = new List<FieldRef>
				{
					new FieldRef { Name = "Is Identifier" }
				},
				IncludeNameInQueryResult = true
			};

			QueryResult result = await objectManager
				.QueryAsync(workspaceId, request, 0, int.MaxValue).ConfigureAwait(false);

			if (result.ResultCount == 0)
			{
				throw InvalidFieldsMappingException.FieldNotFound(fieldName);
			}
			else if (result.ResultCount > 1)
			{
				throw InvalidFieldsMappingException.AmbiguousMatch(fieldName);
			}

			RelativityObject field = result.Objects.Single();

			var fieldEntry = new FieldEntry
			{
				DisplayName = field.Name,
				FieldIdentifier = field.ArtifactID,
				IsIdentifier = (bool) field.FieldValues.Single().Value
			};

			if (fieldEntry.IsIdentifier)
			{
				throw InvalidFieldsMappingException.FieldIsIdentifier(fieldEntry.FieldIdentifier);
			}

			return fieldEntry;
		}
	}
}
