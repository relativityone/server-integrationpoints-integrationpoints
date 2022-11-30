using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal sealed class DestinationWorkspaceObjectTypesCreationExecutor : IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>
    {
        private const int _DEFAULT_FIELD_WIDTH = 100;
        private const int _DEFAULT_FIXEDLENGTH_FIELD_LENGTH = 255;

        private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
        private const string _SOURCE_WORKSPACE_CASEID_FIELD_NAME = "Source Workspace Artifact ID";
        private const string _SOURCE_WORKSPACE_CASENAME_FIELD_NAME = "Source Workspace Name";
        private const string _SOURCE_WORKSPACE_INSTANCENAME_FIELD_NAME = "Source Instance Name";

        private const string _SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";
        private const string _SOURCE_JOB_JOBHISTORYID_FIELD_NAME = "Job History Artifact ID";
        private const string _SOURCE_JOB_JOBHISTORYNAME_FIELD_NAME = "Job History Name";

        private readonly ISyncObjectTypeManager _syncObjectTypeManager;
        private readonly ISyncFieldManager _syncFieldManager;
        private readonly IAPILog _logger;

        private static readonly Guid SourceWorkspaceObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
        private static readonly Guid SourceJobObjectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

        private static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
        private static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
        private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
        private static readonly Guid SourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");

        private static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
        private static readonly Guid JobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
        private static readonly Guid JobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");

        public DestinationWorkspaceObjectTypesCreationExecutor(ISyncObjectTypeManager syncObjectTypeManager, ISyncFieldManager syncFieldManager, IAPILog logger)
        {
            _syncObjectTypeManager = syncObjectTypeManager;
            _syncFieldManager = syncFieldManager;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IDestinationWorkspaceObjectTypesCreationConfiguration configuration, CompositeCancellationToken token)
        {
            int destinationWorkspaceArtifactId = configuration.DestinationWorkspaceArtifactId;
            _logger.LogInformation("Creating object types and fields in destination workspace Artifact ID: {destinationWorkspaceArtifactId}", destinationWorkspaceArtifactId);

            try
            {
                int sourceCaseObjectTypeArtifactId = await CreateSourceCaseObjectTypeAndFieldsAsync(destinationWorkspaceArtifactId).ConfigureAwait(false);
                await CreateSourceJobObjectTypeAndFieldsAsync(sourceCaseObjectTypeArtifactId, destinationWorkspaceArtifactId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create object types in destination workspace Artifact ID: {destinationWorkspaceArtifactId}", destinationWorkspaceArtifactId);
                return ExecutionResult.Failure(ex);
            }

            return ExecutionResult.Success();
        }

        private async Task<int> CreateSourceCaseObjectTypeAndFieldsAsync(int destinationWorkspaceArtifactId)
        {
            ObjectTypeRequest sourceCaseObjectTypeRequest = GetObjectTypeRequest(_SOURCE_WORKSPACE_OBJECT_TYPE_NAME);
            sourceCaseObjectTypeRequest.ParentObjectType.Value.Name = "Workspace";
            int sourceCaseObjectTypeArtifactId = await _syncObjectTypeManager.EnsureObjectTypeExistsAsync(
                destinationWorkspaceArtifactId,
                SourceWorkspaceObjectTypeGuid, sourceCaseObjectTypeRequest).ConfigureAwait(false);

            await _syncFieldManager.EnsureFieldsExistAsync(
                destinationWorkspaceArtifactId,
                GetSourceWorkspaceRdoFieldsRequests(sourceCaseObjectTypeArtifactId)).ConfigureAwait(false);
            await _syncFieldManager.EnsureFieldsExistAsync(
                destinationWorkspaceArtifactId,
                GetDocumentFieldRequest(sourceCaseObjectTypeArtifactId, _SOURCE_WORKSPACE_OBJECT_TYPE_NAME, SourceWorkspaceFieldOnDocumentGuid)).ConfigureAwait(false);
            return sourceCaseObjectTypeArtifactId;
        }

        private async Task CreateSourceJobObjectTypeAndFieldsAsync(int sourceCaseObjectTypeArtifactId, int destinationWorkspaceArtifactId)
        {
            ObjectTypeRequest sourceJobObjectTypeRequest = GetObjectTypeRequest(_SOURCE_JOB_OBJECT_TYPE_NAME);
            sourceJobObjectTypeRequest.ParentObjectType.Value.ArtifactID = sourceCaseObjectTypeArtifactId;
            int sourceJobObjectTypeArtifactId = await _syncObjectTypeManager
                .EnsureObjectTypeExistsAsync(destinationWorkspaceArtifactId, SourceJobObjectTypeGuid, sourceJobObjectTypeRequest).ConfigureAwait(false);

            await _syncFieldManager.EnsureFieldsExistAsync(
                destinationWorkspaceArtifactId,
                GetSourceJobRdoFieldsRequests(sourceJobObjectTypeArtifactId)).ConfigureAwait(false);
            await _syncFieldManager.EnsureFieldsExistAsync(
                destinationWorkspaceArtifactId,
                GetDocumentFieldRequest(sourceJobObjectTypeArtifactId, _SOURCE_JOB_OBJECT_TYPE_NAME, JobHistoryFieldOnDocumentGuid)).ConfigureAwait(false);
        }

        private static ObjectTypeRequest GetObjectTypeRequest(string name)
        {
            return new ObjectTypeRequest()
            {
                ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()),
                Name = name,
                CopyInstancesOnParentCopy = false,
                CopyInstancesOnCaseCreation = false,
                EnableSnapshotAuditingOnDelete = false,
                PivotEnabled = true,
                SamplingEnabled = false,
                PersistentListsEnabled = false
            };
        }

        private static IDictionary<Guid, BaseFieldRequest> GetDocumentFieldRequest(int objectTypeArtifactId, string name, Guid guid)
        {
            ObjectTypeIdentifier documentObjectType = new ObjectTypeIdentifier()
            {
                ArtifactTypeID = (int)ArtifactType.Document
            };

            ObjectTypeIdentifier associativeObjectType = new ObjectTypeIdentifier()
            {
                ArtifactID = objectTypeArtifactId
            };

            return new Dictionary<Guid, BaseFieldRequest>()
            {
                {
                    guid,
                    new MultipleObjectFieldRequest()
                    {
                        Name = name,
                        ObjectType = documentObjectType,
                        AssociativeObjectType = associativeObjectType,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        AvailableInFieldTree = false,
                        IsRequired = false,
                        Width = _DEFAULT_FIELD_WIDTH,
                        FilterType = FilterType.Popup,
                        OverlayBehavior = OverlayBehavior.ReplaceValues
                    }
                }
            };
        }

        private static IDictionary<Guid, BaseFieldRequest> GetSourceWorkspaceRdoFieldsRequests(int objectTypeArtifactId)
        {
            ObjectTypeIdentifier objectType = new ObjectTypeIdentifier()
            {
                ArtifactID = objectTypeArtifactId
            };
            return new Dictionary<Guid, BaseFieldRequest>()
            {
                {
                    CaseIdFieldNameGuid,
                    new WholeNumberFieldRequest()
                    {
                        Name = _SOURCE_WORKSPACE_CASEID_FIELD_NAME,
                        ObjectType = objectType,
                        IsRequired = true,
                        IsLinked = false,
                        OpenToAssociations = false,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        Width = _DEFAULT_FIELD_WIDTH,
                        Wrapping = false
                    }
                },
                {
                    CaseNameFieldNameGuid,
                    new FixedLengthFieldRequest()
                    {
                        Name = _SOURCE_WORKSPACE_CASENAME_FIELD_NAME,
                        ObjectType = objectType,
                        IsRequired = true,
                        IncludeInTextIndex = false,
                        IsLinked = false,
                        AllowHtml = false,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        OpenToAssociations = false,
                        Width = _DEFAULT_FIELD_WIDTH,
                        Wrapping = false,
                        HasUnicode = false,
                        Length = _DEFAULT_FIXEDLENGTH_FIELD_LENGTH
                    }
                },
                {
                    InstanceNameFieldGuid,
                    new FixedLengthFieldRequest()
                    {
                        Name = _SOURCE_WORKSPACE_INSTANCENAME_FIELD_NAME,
                        ObjectType = objectType,
                        IsRequired = true,
                        IncludeInTextIndex = false,
                        IsLinked = false,
                        AllowHtml = false,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        OpenToAssociations = false,
                        Width = _DEFAULT_FIELD_WIDTH,
                        Wrapping = false,
                        HasUnicode = false,
                        Length = _DEFAULT_FIXEDLENGTH_FIELD_LENGTH
                    }
                }
            };
        }

        private static IDictionary<Guid, BaseFieldRequest> GetSourceJobRdoFieldsRequests(int objectTypeArtifactId)
        {
            ObjectTypeIdentifier objectType = new ObjectTypeIdentifier()
            {
                ArtifactID = objectTypeArtifactId
            };
            return new Dictionary<Guid, BaseFieldRequest>()
            {
                {
                    JobHistoryIdFieldGuid,
                    new WholeNumberFieldRequest()
                    {
                        Name = _SOURCE_JOB_JOBHISTORYID_FIELD_NAME,
                        ObjectType = objectType,
                        IsRequired = true,
                        IsLinked = false,
                        OpenToAssociations = false,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        Width = _DEFAULT_FIELD_WIDTH,
                        Wrapping = false
                    }
                },
                {
                    JobHistoryNameFieldGuid,
                    new FixedLengthFieldRequest()
                    {
                        Name = _SOURCE_JOB_JOBHISTORYNAME_FIELD_NAME,
                        ObjectType = objectType,
                        IsRequired = true,
                        IncludeInTextIndex = false,
                        IsLinked = false,
                        AllowHtml = false,
                        AllowGroupBy = false,
                        AllowPivot = false,
                        OpenToAssociations = false,
                        Width = _DEFAULT_FIELD_WIDTH,
                        Wrapping = false,
                        HasUnicode = false,
                        Length = _DEFAULT_FIXEDLENGTH_FIELD_LENGTH
                    }
                }
            };
        }
    }
}
