using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Objects.Exceptions;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceObjectTypesCreationExecutor : IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>
	{
		private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
		private const string _SOURCEWORKSPACE_CASEID_FIELD_NAME = "Source Workspace Artifact ID";
		private const string _SOURCEWORKSPACE_CASENAME_FIELD_NAME = "Source Workspace Name";
		private const string _SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME = "Source Instance Name";
		private readonly ISyncObjectTypeManager _syncObjectTypeManager;
		private readonly ISyncFieldManager _syncFieldManager;
		private readonly ISyncLog _logger;
		private static readonly Guid SourceWorkspaceObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
		private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid SourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");

		public DestinationWorkspaceObjectTypesCreationExecutor(ISyncObjectTypeManager syncObjectTypeManager, ISyncFieldManager syncFieldManager, ISyncLog logger)
		{
			_syncObjectTypeManager = syncObjectTypeManager;
			_syncFieldManager = syncFieldManager;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDestinationWorkspaceObjectTypesCreationConfiguration configuration, CancellationToken token)
		{
			try
			{
				int workspaceObjectTypeArtifactId = await GetWorkspaceObjectTypeArtifactIdAsync(configuration.DestinationWorkspaceArtifactId).ConfigureAwait(false);
				ObjectTypeRequest objectTypeRequest = GetObjectTypeRequest(_SOURCE_WORKSPACE_OBJECT_TYPE_NAME, workspaceObjectTypeArtifactId);
				int sourceCaseObjectTypeArtifactId = await _syncObjectTypeManager.EnsureObjectTypeExistsAsync(configuration.DestinationWorkspaceArtifactId,
					SourceWorkspaceObjectTypeGuid, objectTypeRequest).ConfigureAwait(false);
				await _syncFieldManager.EnsureFieldsExistAsync(configuration.DestinationWorkspaceArtifactId, GetSourceWorkspaceRdoFieldsRequests(sourceCaseObjectTypeArtifactId)).ConfigureAwait(false);
				await _syncFieldManager.EnsureFieldsExistAsync(configuration.DestinationWorkspaceArtifactId, GetSourceWorkspaceDocumentFieldsRequests(sourceCaseObjectTypeArtifactId)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create object types in destination workspace Artifact ID: {destinationWorkspaceArtifactId}", configuration.DestinationWorkspaceArtifactId);
				return ExecutionResult.Failure(ex);
			}

			return ExecutionResult.Success();
		}

		private async Task<int> GetWorkspaceObjectTypeArtifactIdAsync(int workspaceArtifactId)
		{
			const string workspaceObjectTypeName = "Workspace";
			QueryResult queryResult = await _syncObjectTypeManager.QueryObjectTypeByNameAsync(workspaceArtifactId, workspaceObjectTypeName).ConfigureAwait(false);
			if (!queryResult.Objects.Any())
			{
				throw new ArtifactNotFoundException($"Cannot find object type name: '{workspaceObjectTypeName}'");
			}
			else
			{
				return queryResult.Objects.First().ArtifactID;
			}
		}

		private ObjectTypeRequest GetObjectTypeRequest(string name, int parentArtifactId)
		{
			return new ObjectTypeRequest()
			{
				ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()
				{
					ArtifactID = parentArtifactId
				}),
				Name = name,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnCaseCreation = false,
				EnableSnapshotAuditingOnDelete = false,
				PivotEnabled = true,
				SamplingEnabled = false,
				PersistentListsEnabled = false
			};
		}

		private IDictionary<Guid, BaseFieldRequest> GetSourceWorkspaceDocumentFieldsRequests(int objectTypeArtifactTypeId)
		{
			ObjectTypeIdentifier documentObjectType = new ObjectTypeIdentifier()
			{
				ArtifactTypeID = (int)ArtifactType.Document
			};

			ObjectTypeIdentifier associativeObjectType = new ObjectTypeIdentifier()
			{
				ArtifactID = objectTypeArtifactTypeId
			};

			const int width = 100;

			return new Dictionary<Guid, BaseFieldRequest>()
			{
				{
					SourceWorkspaceFieldOnDocumentGuid,
					new MultipleObjectFieldRequest()
					{
						Name = _SOURCE_WORKSPACE_OBJECT_TYPE_NAME,
						ObjectType = documentObjectType,
						AssociativeObjectType = associativeObjectType,
						AllowGroupBy = false,
						AllowPivot = false,
						AvailableInFieldTree = false,
						IsRequired = false,
						Width = width
					}
				}
			};
		}

		private IDictionary<Guid, BaseFieldRequest> GetSourceWorkspaceRdoFieldsRequests(int objectTypeArtifactId)
		{
			ObjectTypeIdentifier objectType = new ObjectTypeIdentifier()
			{
				ArtifactID = objectTypeArtifactId
			};
			const int width = 100;
			const int length = 255;
			return new Dictionary<Guid, BaseFieldRequest>()
			{
				{
					CaseIdFieldNameGuid,
					new WholeNumberFieldRequest()
					{
						Name = _SOURCEWORKSPACE_CASEID_FIELD_NAME,
						ObjectType = objectType,
						IsRequired = true,
						IsLinked = false,
						OpenToAssociations = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						Width = width,
						Wrapping = false
					}
				},
				{
					CaseNameFieldNameGuid,
					new FixedLengthFieldRequest()
					{
						Name = _SOURCEWORKSPACE_CASENAME_FIELD_NAME,
						ObjectType = objectType,
						IsRequired = true,
						IncludeInTextIndex = false,
						IsLinked = false,
						AllowHtml = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						OpenToAssociations = false,
						Width = width,
						Wrapping = false,
						HasUnicode = false,
						Length = length
					}
				},
				{
					InstanceNameFieldGuid,
					new FixedLengthFieldRequest()
					{
						Name = _SOURCEWORKSPACE_INSTANCENAME_FIELD_NAME,
						ObjectType = objectType,
						IsRequired = true,
						IncludeInTextIndex = false,
						IsLinked = false,
						AllowHtml = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						OpenToAssociations = false,
						Width = width,
						Wrapping = false,
						HasUnicode = false,
						Length = length
					}
				}
			};
		}
	}
}