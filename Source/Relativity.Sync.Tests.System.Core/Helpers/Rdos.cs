using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;
using Relativity.Services.User;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Sync.Utils;
using User = Relativity.Services.User.User;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    internal static class Rdos
    {
        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

        private static readonly Guid _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID =
            new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");

        private static readonly Guid _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID =
            new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

        private static readonly Guid _DESTINATION_WORKSPACE_OBJECT_TYPE_GUID =
            new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");

        private static readonly Guid _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID =
            new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");

        private static readonly Guid _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID =
            new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");

        private static readonly Guid _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID =
            new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");

        private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID =
            new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");

        private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID =
            new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");

        private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_GUID =
            new Guid("323458db-8a06-464b-9402-af2516cf47e0");

        private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_GUID =
            new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");

        private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_GUID =
            new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

        private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_GUID =
            new Guid("348d7394-2658-4da4-87d0-8183824adf98");

        private static readonly Guid _DESTINATION_WORKSPACE_NAME_GUID =
            new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");

        private static readonly Guid JobHistoryMultiObjectFieldGuid = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");

        public static async Task<int> CreateJobHistoryInstanceAsync(ServiceFactory serviceFactory, int workspaceId, string name = "Name", Guid? jobHistoryTypeGuid = null)
        {
            RelativityObject result = await CreateJobHistoryRelativityObjectInstanceAsync(serviceFactory, workspaceId, jobHistoryTypeGuid ?? new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9"), name).ConfigureAwait(false);

            return result.ArtifactID;
        }

        public static async Task<RelativityObject> CreateJobHistoryRelativityObjectInstanceAsync(ServiceFactory serviceFactory, int workspaceId, Guid jobHistoryTypeGuid, string name = "Name")
        {
            using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                CreateRequest request = new CreateRequest
                {
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Value = name,
                            Field = new FieldRef
                            {
                                Name = "Name"
                            }
                        }
                    },
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = jobHistoryTypeGuid
                    }
                };
                CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
                return result.Object;
            }
        }

        public static async Task<int> CreateRelativitySourceCaseInstanceAsync(
            ServiceFactory serviceFactory,
            int destinationWorkspaceArtifactId, RelativitySourceCaseTag tag)
        {
            using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                CreateRequest request = new CreateRequest
                {
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Name = "Name" },
                            Value = tag.Name
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID },
                            Value = tag.SourceWorkspaceArtifactId
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID },
                            Value = tag.SourceWorkspaceName
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID },
                            Value = tag.SourceInstanceName
                        }
                    },
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID
                    }
                };
                CreateResult result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request)
                    .ConfigureAwait(false);
                return result.Object.ArtifactID;
            }
        }

        public static async Task<int> CreateRelativitySourceJobInstanceAsync(
            ServiceFactory serviceFactory,
            int destinationWorkspaceArtifactId, RelativitySourceJobTag tag)
        {
            using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                CreateRequest request = new CreateRequest
                {
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Name = "Name" },
                            Value = tag.Name
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID },
                            Value = tag.JobHistoryArtifactId
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID },
                            Value = tag.JobHistoryName
                        },
                    },
                    ParentObject = new RelativityObjectRef { ArtifactID = tag.SourceCaseTagArtifactId },
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID
                    }
                };
                CreateResult result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request)
                    .ConfigureAwait(false);
                return result.Object.ArtifactID;
            }
        }

        public static async Task<int> CreateDestinationWorkspaceTagInstanceAsync(
            ServiceFactory serviceFactory,
            int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
        {
            const string destinationInstanceName = "This Instance";
            string tagName = $"Test {destinationWorkspaceName}";

            using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                CreateRequest request = new CreateRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = _DESTINATION_WORKSPACE_OBJECT_TYPE_GUID
                    },
                    FieldValues = new[]
                    {
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_GUID },
                            Value = destinationWorkspaceArtifactId
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_GUID },
                            Value = -1
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_GUID },
                            Value = destinationInstanceName
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_GUID },
                            Value = destinationWorkspaceName
                        },
                        new FieldRefValuePair
                        {
                            Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_NAME_GUID },
                            Value = tagName
                        }
                    }
                };

                CreateResult result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request)
                    .ConfigureAwait(false);
                return result.Object.ArtifactID;
            }
        }

        public static async Task<int> GetSavedSearchInstanceAsync(ServiceFactory serviceFactory, int workspaceId,
            string name = "All Documents")
        {
            using (IKeywordSearchManager keywordSearchManager = serviceFactory.CreateProxy<IKeywordSearchManager>())
            {
                Services.Query request = new Services.Query
                {
                    Condition = $"(('Name' == '{name}'))"
                };
                KeywordSearchQueryResultSet result =
                    await keywordSearchManager.QueryAsync(workspaceId, request).ConfigureAwait(false);
                if (result.TotalCount == 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot find saved search '{name}' in workspace {workspaceId}");
                }

                return result.Results.First().Artifact.ArtifactID;
            }
        }

        public static async Task<int> GetRootFolderInstanceAsync(ServiceFactory serviceFactory, int workspaceId)
        {
            using (IFolderManager folderManager = serviceFactory.CreateProxy<IFolderManager>())
            {
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
                return rootFolder.ArtifactID;
            }
        }

        public static async Task<int> CreateFolderInstanceAsync(ServiceFactory serviceFactory, int workspaceId,
            string folderName)
        {
            using (IFolderManager folderManager = serviceFactory.CreateProxy<IFolderManager>())
            {
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
                var folder = new Folder
                {
                    ParentFolder = new FolderRef(rootFolder.ArtifactID),
                    Name = folderName
                };
                int childFolderArtifactId =
                    await folderManager.CreateSingleAsync(workspaceId, folder).ConfigureAwait(false);
                return childFolderArtifactId;
            }
        }

        public static async Task<string> GetFolderPathSourceFieldNameAsync(ServiceFactory serviceFactory, int workspaceId)
        {
            using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Name = "Field"
                    },
                    Condition = "(('Name' == 'Document Folder Path'))",
                    Fields = new List<FieldRef>
                    {
                        new FieldRef
                        {
                            Name = "Field Type"
                        }
                    },
                    IncludeNameInQueryResult = true
                };
                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
                return result.Objects.First().Name;
            }
        }

        public static Task<IList<int>> QueryDocumentIdsAsync(ServiceFactory serviceFactory, int workspaceId)
        {
            return QueryDocumentIdsAsync(serviceFactory, workspaceId, string.Empty);
        }

        public static async Task<IList<int>> QueryDocumentIdsAsync(ServiceFactory serviceFactory, int workspaceId,
            string condition)
        {
            IList<RelativityObject> documents =
                await QueryDocumentsAsync(serviceFactory, workspaceId, condition).ConfigureAwait(false);
            IList<int> identifiers = documents.Select(x => x.ArtifactID).ToList();
            return identifiers;
        }

        public static Task<IList<string>> GetAllDocumentNamesAsync(ServiceFactory serviceFactory, int workspaceId)
        {
            return QueryDocumentNamesAsync(serviceFactory, workspaceId, string.Empty);
        }

        public static async Task<IList<int>> QueryDocumentIdentifiersAsync(
            ServiceFactory serviceFactory,
            int workspaceId, string condition)
        {
            IList<RelativityObject> documents =
                await QueryDocumentsAsync(serviceFactory, workspaceId, condition).ConfigureAwait(false);

            return documents.Select(x => x.ArtifactID).ToList();
        }

        public static async Task<IList<string>> QueryDocumentNamesAsync(
            ServiceFactory serviceFactory,
            int workspaceId, string condition)
        {
            IList<RelativityObject> documents =
                await QueryDocumentsAsync(serviceFactory, workspaceId, condition).ConfigureAwait(false);

            return documents.Select(x => x.Name).ToList();
        }

        public static async Task TagDocumentsAsync(ServiceFactory serviceFactory, int workspaceId, int jobHistoryArtifactId, int numberOfDocuments)
        {
            foreach (var documentArtifactId in (await QueryDocumentIdsAsync(serviceFactory, workspaceId).ConfigureAwait(false)).Take(numberOfDocuments))
            {
                UpdateRequest tagUpdateRequest = CreateTagUpdateRequest(documentArtifactId, jobHistoryArtifactId);

                using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
                {
                    await objectManager.UpdateAsync(workspaceId, tagUpdateRequest).ConfigureAwait(false);
                }
            }
        }

        private static UpdateRequest CreateTagUpdateRequest(int documentArtifactId, int jobHistoryArtifactId)
        {
            return new UpdateRequest
            {
                FieldValues = new[]
                {
                    new FieldRefValuePair
                    {
                        Field = new FieldRef { Guid = JobHistoryMultiObjectFieldGuid },
                        Value = new[] { new RelativityObjectRef { ArtifactID = jobHistoryArtifactId } }
                    }
                },
                Object = new RelativityObjectRef
                {
                    ArtifactID = documentArtifactId
                }
            };
        }

        private static SyncConfigurationRdo GetConfiguration(ConfigurationStub configurationStub, ISerializer serializer)
        {
            return new SyncConfigurationRdo
            {
                CreateSavedSearchInDestination = configurationStub.CreateSavedSearchForTags,
                DataDestinationArtifactId = configurationStub.DestinationFolderArtifactId,
                DataDestinationType = DestinationLocationType.Folder,
                DataSourceArtifactId = configurationStub.SavedSearchArtifactId,
                DataSourceType = DataSourceType.SavedSearch,
                DestinationFolderStructureBehavior = configurationStub.DestinationFolderStructureBehavior,
                FolderPathSourceFieldName = configurationStub.FolderPathSourceFieldName,
                DestinationWorkspaceArtifactId = configurationStub.DestinationWorkspaceArtifactId,
                EmailNotificationRecipients = configurationStub.GetNotificationEmails(),
                FieldsMapping = serializer.Serialize(configurationStub.GetFieldMappings()),
                FieldOverlayBehavior = configurationStub.FieldOverlayBehavior,
                ImportOverwriteMode = configurationStub.ImportOverwriteMode,
                MoveExistingDocuments = configurationStub.MoveExistingDocuments,
                NativesBehavior = configurationStub.ImportNativeFileCopyMode,
                RdoArtifactTypeId = configurationStub.RdoArtifactTypeId,
                DestinationRdoArtifactTypeId = configurationStub.DestinationRdoArtifactTypeId,
                JobHistoryId = configurationStub.JobHistoryArtifactId,
                JobHistoryToRetryId = configurationStub.JobHistoryToRetryId,
                ImageImport = configurationStub.ImageImport,
                IncludeOriginalImages = configurationStub.ProductionImagePrecedence is null || configurationStub.IncludeOriginalImageIfNotFoundInProductions,
                ImageFileCopyMode = configurationStub.ImportImageFileCopyMode,
                ProductionImagePrecedence = configurationStub.ProductionImagePrecedence is null ? string.Empty : serializer.Serialize(configurationStub.ProductionImagePrecedence),
                LogItemLevelErrors = configurationStub.LogItemLevelErrors,
                EnableTagging = configurationStub.EnableTagging,

                // Drain-Stop
                SyncStatisticsId = configurationStub.SyncStatisticsId,

                // JobHistoryGuids
                JobHistoryType = configurationStub.JobHistory.TypeGuid,
                JobHistoryGuidTotalField = configurationStub.JobHistory.TotalItemsFieldGuid,
                JobHistoryGuidFailedField = configurationStub.JobHistory.FailedItemsFieldGuid,
                JobHistoryCompletedItemsField = configurationStub.JobHistory.CompletedItemsFieldGuid,
                JobHistoryDestinationWorkspaceInformationField = configurationStub.JobHistory.DestinationWorkspaceInformationGuid,

                // JobHistoryErrorGuids
                JobHistoryErrorType = configurationStub.JobHistoryError.TypeGuid,
                JobHistoryErrorErrorMessages = configurationStub.JobHistoryError.ErrorMessagesGuid,
                JobHistoryErrorErrorStatus = configurationStub.JobHistoryError.ErrorStatusGuid,
                JobHistoryErrorErrorType = configurationStub.JobHistoryError.ErrorTypeGuid,
                JobHistoryErrorName = configurationStub.JobHistoryError.NameGuid,
                JobHistoryErrorSourceUniqueId = configurationStub.JobHistoryError.SourceUniqueIdGuid,
                JobHistoryErrorStackTrace = configurationStub.JobHistoryError.StackTraceGuid,
                JobHistoryErrorTimeStamp = configurationStub.JobHistoryError.TimeStampGuid,
                JobHistoryErrorItemLevelError = configurationStub.JobHistoryError.ItemLevelErrorGuid,
                JobHistoryErrorJobLevelError = configurationStub.JobHistoryError.JobLevelErrorGuid,
                JobHistoryErrorJobHistoryRelation = configurationStub.JobHistoryError.JobHistoryRelationGuid,
                JobHistoryErrorNewChoice = configurationStub.JobHistoryError.NewStatusGuid,

                // DestinationWorkspaceGuids
                DestinationWorkspaceType = configurationStub.DestinationWorkspace.TypeGuid,
                DestinationWorkspaceNameField = configurationStub.DestinationWorkspace.NameGuid,
                DestinationWorkspaceWorkspaceArtifactIdField = configurationStub.DestinationWorkspace.DestinationWorkspaceArtifactIdGuid,
                DestinationWorkspaceDestinationWorkspaceName = configurationStub.DestinationWorkspace.DestinationWorkspaceNameGuid,
                DestinationWorkspaceDestinationInstanceArtifactId = configurationStub.DestinationWorkspace.DestinationInstanceArtifactIdGuid,
                DestinationWorkspaceDestinationInstanceName = configurationStub.DestinationWorkspace.DestinationInstanceNameGuid,
                DestinationWorkspaceOnDocumentField = configurationStub.DestinationWorkspace.DestinationWorkspaceOnDocument,
                JobHistoryOnDocumentField = configurationStub.DestinationWorkspace.JobHistoryOnDocumentGuid,
            };
        }

        public static async Task<IList<RelativityObject>> QueryDocumentsAsync(
            ServiceFactory serviceFactory,
            int workspaceId, string condition)
        {
            using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                var documents = new List<RelativityObject>();
                var queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
                    Condition = condition,
                    IncludeNameInQueryResult = true
                };

                QueryResult queryResult;
                do
                {
                    const int batchSize = 100;
                    queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, documents.Count, batchSize)
                        .ConfigureAwait(false);
                    documents.AddRange(queryResult.Objects);
                }
                while (documents.Count < queryResult.TotalCount);

                return documents;
            }
        }

        public static async Task<RelativityObject> GetJobHistoryAsync(ServiceFactory serviceFactory, int workspaceId, int jobHistoryId, Guid jobHistoryGuid)
        {
            using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = jobHistoryGuid
                    },
                    Condition = $"'ArtifactId' == {jobHistoryId}",
                    Fields = new[]
                    {
                        new FieldRef
                        {
                            Name = "*"
                        }
                    }
                };

                QueryResult result = await objectManager.QueryAsync(workspaceId, request, 1, 1).ConfigureAwait(false);

                return result.Objects.FirstOrDefault();
            }
        }

        public static async Task<User> GetUserAsync(ServiceFactory serviceFactory, int workspaceId)
        {
            using (IUserManager userInfoManager = serviceFactory.CreateProxy<IUserManager>())
            {
                return await userInfoManager.RetrieveCurrentAsync(workspaceId).ConfigureAwait(false);
            }
        }

        public static async Task<int> CreateSyncConfigurationRdoAsync(
            int workspaceId,
            ConfigurationStub configurationStub, IAPILog logger = null, ISerializer serializer = null)
        {
            serializer = serializer ?? new JSONSerializer();
            logger = logger ?? TestLogHelper.GetLogger();
            var rdoManager = new RdoManager(logger, new SourceServiceFactoryStub(), new RdoGuidProvider());
            SyncConfigurationRdo configuration = GetConfiguration(configurationStub, serializer);

            await rdoManager.CreateAsync(workspaceId, configuration).ConfigureAwait(false);

            return configuration.ArtifactId;
        }

        public static Task<int> CreateSyncConfigurationRdoAsync(int workspaceId, int jobHistoryId)
        {
            return CreateSyncConfigurationRdoAsync(workspaceId, new ConfigurationStub
            {
                JobHistoryArtifactId = jobHistoryId
            });
        }

        public static async Task<int> CreateEmptySyncStatisticsRdoAsync(int workspaceId)
        {
            IRdoManager rdoManager = CreateRdoManager();

            SyncStatisticsRdo syncStatistics = new SyncStatisticsRdo();

            await rdoManager.CreateAsync(workspaceId, syncStatistics).ConfigureAwait(false);

            return syncStatistics.ArtifactId;
        }

        public static Task<TRdo> ReadRdoAsync<TRdo>(int workspaceId, int artifactId)
            where TRdo : IRdoType, new()
        {
            IRdoManager rdoManager = CreateRdoManager();

            return rdoManager.GetAsync<TRdo>(workspaceId, artifactId);
        }

        private static IRdoManager CreateRdoManager()
        {
            IAPILog log = TestLogHelper.GetLogger();
            return new RdoManager(log, new SourceServiceFactoryStub(), new RdoGuidProvider());
        }
    }
}
