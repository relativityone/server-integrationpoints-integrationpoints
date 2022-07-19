using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Folder;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.WorkspaceGenerator.Fields;
using Relativity.Sync.WorkspaceGenerator.Extensions;
using FieldType = Relativity.Services.FieldType;

namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    public class WorkspaceService : IWorkspaceService
    {
        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;
        private const string _WORKSPACE_GENERATOR_FIELD_KEYWORD = "Workspace Generator";

        private readonly IServiceFactory _serviceFactory;
        private readonly Random _random = new Random();
        
        private readonly ObjectTypeIdentifier _documentObjectTypeIdentifier = new ObjectTypeIdentifier()
        {
            ArtifactTypeID = (int)ArtifactType.Document
        };

        public WorkspaceService(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<IEnumerable<WorkspaceRef>> GetAllActiveAsync()
        {
            using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
            {
                return await workspaceManager.RetrieveAllActive().ConfigureAwait(false);
            }
        }

        public async Task<WorkspaceRef> GetWorkspaceAsync(int workspaceId)
        {
            Console.WriteLine($"Reading workspace {workspaceId}");
            IEnumerable<WorkspaceRef> activeWorkspaces = await GetAllActiveAsync().ConfigureAwait(false);
            return activeWorkspaces.Single(x => x.ArtifactID == workspaceId);
        }

        public async Task<WorkspaceRef> GetWorkspaceAsync(string workspaceName)
        {
            Console.WriteLine($"Reading workspace {workspaceName}");
            IEnumerable<WorkspaceRef> activeWorkspaces = await GetAllActiveAsync().ConfigureAwait(false);
            return activeWorkspaces.FirstOrDefault(x => x.Name == workspaceName);
        }

        public async Task<WorkspaceRef> CreateWorkspaceAsync(string name, string templateWorkspaceName)
        {
            using (var workspaceManager = _serviceFactory.CreateProxy<IWorkspaceManager>())
            {
                Console.WriteLine($"Gathering all active workspaces");
                IEnumerable<WorkspaceRef> activeWorkspaces = await GetAllActiveAsync().ConfigureAwait(false);
                WorkspaceRef template = activeWorkspaces.FirstOrDefault(x => x.Name == templateWorkspaceName);

                if (template == null)
                {
                    throw new Exception($"Cannot find workspace name: '{templateWorkspaceName}'");
                }

                Console.WriteLine($"Template workspace Artifact ID: {template.ArtifactID}");

                Console.WriteLine($"Creating new workspace: '{name}'");
                WorkspaceRef workspaceRef = await workspaceManager.CreateWorkspaceAsync(new WorkspaceSetttings()
                {
                    Name = name,
                    TemplateArtifactId = template.ArtifactID,
                    EnableDataGrid = true
                }).ConfigureAwait(false);

                return workspaceRef;
            }
        }

        public async Task<int> GetRootFolderArtifactIDAsync(int workspaceID)
        {
            using (IFolderManager folderManager = _serviceFactory.CreateProxy<IFolderManager>())
            {
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceID).ConfigureAwait(false);
                return rootFolder.ArtifactID;
            }
        }

        public async Task<bool> GetExtractedTextFieldEnableForDataGridAsync(int workspaceId)
        {
            string fieldName = Consts.ExtractedTextFieldName;

            using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            using (IFieldManager fieldManager = _serviceFactory.CreateProxy<IFieldManager>())
            {
                QueryRequest fieldRequest = CreateObjectManagerArtifactIdQueryRequest(fieldName);
                QueryResult fieldQueryResult = await objectManager.QueryAsync(workspaceId, fieldRequest, 0, 1).ConfigureAwait(false);
                var fieldArtifactId = fieldQueryResult.Objects.Single().ArtifactID;

                FieldResponse fieldResponse = await fieldManager.ReadAsync(workspaceId, fieldArtifactId).ConfigureAwait(false);
                return fieldResponse.EnableDataGrid;
            }
        }

        public async Task EnableExtractedTextFieldForDataGridAsync(int workspaceId)
        {
            string fieldName = Consts.ExtractedTextFieldName;

            using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            using (IFieldManager fieldManager = _serviceFactory.CreateProxy<IFieldManager>())
            {
                QueryRequest fieldRequest = CreateObjectManagerArtifactIdQueryRequest(fieldName);
                QueryResult fieldQueryResult= await objectManager.QueryAsync(workspaceId,fieldRequest,0,1).ConfigureAwait(false);
                var fieldArtifactId = fieldQueryResult.Objects.Single().ArtifactID;

                var longTextFieldRequest = new LongTextFieldRequest()
                {
                    ObjectType = new ObjectTypeIdentifier()
                    {
                        ArtifactTypeID = (int) ArtifactType.Document
                    },
                    Name = $"{fieldName}",
                    EnableDataGrid = true,
                    IncludeInTextIndex = false,
                    FilterType = FilterType.None,
                    AvailableInViewer = true,
                    HasUnicode = true
                };
                await fieldManager.UpdateLongTextFieldAsync(workspaceId, fieldArtifactId, longTextFieldRequest).ConfigureAwait(false);
            }
        }

        public async Task<List<CustomField>> GetAllNonSystemDocumentFieldsAsync(int workspaceId)
        {
            using (IObjectManager objectManager = _serviceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest fieldsQueryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                    Condition = $"'Object Type Artifact Type ID' == {_DOCUMENT_ARTIFACT_TYPE_ID} AND 'Keywords' == '{_WORKSPACE_GENERATOR_FIELD_KEYWORD}'",
                    Fields = new[]
                    {
                        new FieldRef {Name = "Name"},
                        new FieldRef { Name = "Field Type" }
                    }
                };

                IEnumerable<QueryResult> results = await objectManager.QueryAllAsync(workspaceId, fieldsQueryRequest).ConfigureAwait(false);

                return results.SelectMany(x => x.Objects.Select(o => new CustomField(
                    o.FieldValues[0].Value.ToString(),
                    o.FieldValues[1].Value.ToString()
                ))).ToList();
            }
        }

        public async Task CreateFieldsAsync(int workspaceId, IEnumerable<CustomField> fields)
        {
            using (IFieldManager fieldManager = _serviceFactory.CreateProxy<IFieldManager>())
            {
                foreach (CustomField field in fields)
                {
                    Console.WriteLine($"Creating field in workspace. Field name: '{field.Name}'\t\tType: '{field.Type}'");

                    switch (field.Type)
                    {
                        case FieldType.FixedLengthText:
                            await fieldManager
                                .CreateFixedLengthFieldAsync(workspaceId, CreateFixedLengthFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        case FieldType.LongText:
                            await fieldManager
                                .CreateLongTextFieldAsync(workspaceId, CreateLongTextFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        case FieldType.Date:
                            await fieldManager
                                .CreateDateFieldAsync(workspaceId, CreateDateFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        case FieldType.Decimal:
                            await fieldManager
                                .CreateDecimalFieldAsync(workspaceId, CreateDecimalFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        case FieldType.Currency:
                            await fieldManager
                                .CreateCurrencyFieldAsync(workspaceId, CreateCurrencyFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        case FieldType.WholeNumber:
                            await fieldManager
                                .CreateWholeNumberFieldAsync(workspaceId, CreateWholeNumberFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        case FieldType.YesNo:
                            await fieldManager
                                .CreateYesNoFieldAsync(workspaceId, CreateYesNoFieldRequest(field))
                                .ConfigureAwait(false);
                            break;
                        default:
                            throw new Exception($"Field type is not supported by this tool: {field.Type}");
                    }
                }
            }
        }

        private DateFieldRequest CreateDateFieldRequest(CustomField field)
        {
            Formatting formatting = _random.Next(0, int.MaxValue) % 2 == 0 ? Formatting.Date : Formatting.DateTime;

            return new DateFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                Formatting = formatting,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private FixedLengthFieldRequest CreateFixedLengthFieldRequest(CustomField field)
        {
            return new FixedLengthFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                Length = 255,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private LongTextFieldRequest CreateLongTextFieldRequest(CustomField field)
        {
            return new LongTextFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                EnableDataGrid = false,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private DecimalFieldRequest CreateDecimalFieldRequest(CustomField field)
        {
            return new DecimalFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private CurrencyFieldRequest CreateCurrencyFieldRequest(CustomField field)
        {
            return new CurrencyFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private WholeNumberFieldRequest CreateWholeNumberFieldRequest(CustomField field)
        {
            return new WholeNumberFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private YesNoFieldRequest CreateYesNoFieldRequest(CustomField field)
        {
            return new YesNoFieldRequest()
            {
                ObjectType = _documentObjectTypeIdentifier,
                Name = field.Name,
                Keywords = _WORKSPACE_GENERATOR_FIELD_KEYWORD
            };
        }

        private QueryRequest CreateObjectManagerArtifactIdQueryRequest(string fieldName)
        {
            QueryRequest artifactIdRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                Condition = $"'Object Type Artifact Type ID' == {_DOCUMENT_ARTIFACT_TYPE_ID} AND 'Name' == '{fieldName}'",
                Fields = new[]
                {
                    new FieldRef {Name = "ArtifactID"}
                }
            };
            return artifactIdRequest;
        }
    }
}