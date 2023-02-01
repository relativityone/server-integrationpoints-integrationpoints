using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;
using User = Relativity.Services.User.User;

namespace Relativity.Sync.Tests.System.Core
{
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal abstract class SystemTest : IDisposable
    {
        protected readonly int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

        protected ServiceFactory ServiceFactory { get; private set; }

        protected TestEnvironment Environment { get; private set; }

        protected ImportHelper ImportHelper { get; private set; }

        protected User User { get; private set; }

        protected IAPILog Logger { get; private set; }

        [OneTimeSetUp]
        public async Task SuiteSetup()
        {
            ServiceFactory = new ServiceFactoryFromAppConfig().CreateServiceFactory();
            User = await Rdos.GetUserAsync(ServiceFactory, 0).ConfigureAwait(false);
            Environment = new TestEnvironment();
            ImportHelper = new ImportHelper(ServiceFactory);
            Logger = TestLogHelper.GetLogger();

            await SetRelativityWebApiCredentialsProviderAsync();

            Logger.LogInformation("Invoking ChildSuiteSetup");
            await ChildSuiteSetup().ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task SuiteTeardown()
        {
            await ChildSuiteTeardown().ConfigureAwait(false);

            if (TestContext.CurrentContext.Result.FailCount == 0)
            {
                await Environment.DoCleanupAsync().ConfigureAwait(false);
            }
        }

        protected virtual Task ChildSuiteSetup()
        {
            return Task.CompletedTask;
        }

        protected virtual Task ChildSuiteTeardown()
        {
            return Task.CompletedTask;
        }

        protected async Task<List<FieldMap>> GetDocumentIdentifierMappingAsync(int sourceWorkspaceId, int targetWorkspaceId)
        {
            return await GetIdentifierMappingAsync(sourceWorkspaceId, _DOCUMENT_ARTIFACT_TYPE_ID, targetWorkspaceId);
        }

        protected async Task<List<FieldMap>> GetIdentifierMappingAsync(int sourceWorkspaceId, int artifactTypeId, int targetWorkspaceId)
        {
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest query = PrepareIdentifierFieldsQueryRequest(artifactTypeId);
                QueryResult sourceQueryResult = await objectManager.QueryAsync(sourceWorkspaceId, query, 0, 1).ConfigureAwait(false);
                QueryResult destinationQueryResult = await objectManager.QueryAsync(targetWorkspaceId, query, 0, 1).ConfigureAwait(false);

                return new List<FieldMap>
                {
                    new FieldMap
                    {
                        SourceField = new FieldEntry
                        {
                            DisplayName = sourceQueryResult.Objects.First()["Name"].Value.ToString(),
                            FieldIdentifier = sourceQueryResult.Objects.First().ArtifactID,
                            IsIdentifier = true
                        },
                        DestinationField = new FieldEntry
                        {
                            DisplayName = destinationQueryResult.Objects.First()["Name"].Value.ToString(),
                            FieldIdentifier = destinationQueryResult.Objects.First().ArtifactID,
                            IsIdentifier = true
                        },
                        FieldMapType = FieldMapType.Identifier
                    }
                };
            }
        }

        private QueryRequest PrepareIdentifierFieldsQueryRequest(int artifactTypeId)
        {
            int fieldArtifactTypeID = (int)ArtifactType.Field;
            QueryRequest queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = fieldArtifactTypeID
                },
                Condition = $"'FieldArtifactTypeID' == {artifactTypeId} and 'Is Identifier' == true",
                Fields = new[] { new FieldRef { Name = "Name" } },
                IncludeNameInQueryResult = true
            };

            return queryRequest;
        }

        protected async Task<List<FieldMap>> GetExtractedTextMappingAsync(int sourceWorkspaceId, int destinationWorkspaceId)
        {
            List<FieldMap> indentifierMapping = await GetIdentifierMappingAsync(sourceWorkspaceId, _DOCUMENT_ARTIFACT_TYPE_ID, destinationWorkspaceId);

            List<FieldMap> extractedTextMapping = await GetFieldsMappingAsync(sourceWorkspaceId, destinationWorkspaceId, new[] { "Extracted Text" }).ConfigureAwait(false);

            return indentifierMapping.Concat(extractedTextMapping).ToList();
        }

        protected async Task<List<FieldMap>> GetFieldsMappingAsync(int sourceWorkspaceId, int destinationWorkspaceId, string[] fieldNames)
        {
            using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                QueryRequest query = PrepareFieldsQueryRequest(fieldNames);
                QueryResult sourceQueryResult = await objectManager.QueryAsync(sourceWorkspaceId, query, 0, int.MaxValue).ConfigureAwait(false);
                QueryResult destinationQueryResult = await objectManager.QueryAsync(destinationWorkspaceId, query, 0, int.MaxValue).ConfigureAwait(false);

                if (sourceQueryResult.ResultCount != destinationQueryResult.ResultCount)
                {
                    throw new InvalidOperationException("Error occurred in FieldsMapping preparation. Source Workspace fields count don't match with Destination Workspace fields");
                }

                List<FieldMap> fieldMap = new List<FieldMap>(sourceQueryResult.ResultCount);
                for (int i = 0; i < sourceQueryResult.ResultCount; ++i)
                {
                    fieldMap.Add(new FieldMap
                    {
                        SourceField = new FieldEntry
                        {
                            DisplayName = sourceQueryResult.Objects[i]["Name"].Value.ToString(),
                            FieldIdentifier = sourceQueryResult.Objects[i].ArtifactID,
                            IsIdentifier = false
                        },
                        DestinationField = new FieldEntry
                        {
                            DisplayName = destinationQueryResult.Objects[i]["Name"].Value.ToString(),
                            FieldIdentifier = destinationQueryResult.Objects[i].ArtifactID,
                            IsIdentifier = false
                        },
                        FieldMapType = FieldMapType.None
                    });
                }

                return fieldMap;
            }
        }

        private QueryRequest PrepareFieldsQueryRequest(params string[] fieldNames)
        {
            string fields = string.Join(",", fieldNames.Select(x => $"'{x}'"));

            int fieldArtifactTypeID = (int)ArtifactType.Field;
            QueryRequest queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = fieldArtifactTypeID
                },
                Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} and 'Name' IN [{fields}]",
                Fields = new[] { new FieldRef { Name = "Name" } },
                IncludeNameInQueryResult = true
            };

            return queryRequest;
        }

        protected async Task<ProductionDto> CreateAndImportProductionAsync(int workspaceId, Dataset dataset, string productionName = "")
        {
            if (string.IsNullOrEmpty(productionName))
            {
                productionName = dataset.Name.Replace('\\', '-') + "_" + DateTime.Now.ToLongTimeString() + "_" + DateTime.Now.Ticks;
            }

            int productionId = await Environment.CreateProductionAsync(workspaceId, productionName).ConfigureAwait(false);

            ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
            await ImportHelper.ImportDataAsync(workspaceId, dataTableWrapper, productionId).ConfigureAwait(false);

            return new ProductionDto { ArtifactId = productionId, Name = productionName };
        }

        protected void PrepareSyncConfigurationAndAssignId(ConfigurationStub configuration)
        {
            int syncConfigurationArtifactId =
             Rdos.CreateSyncConfigurationRdoAsync(
                 configuration.SourceWorkspaceArtifactId,
                 configuration,
                 TestLogHelper.GetLogger())
             .GetAwaiter().GetResult();

            configuration.SyncConfigurationArtifactId = syncConfigurationArtifactId;
        }

        protected virtual void Dispose(bool disposing)
        {
            Environment?.Dispose();
        }

        private static async Task SetRelativityWebApiCredentialsProviderAsync()
        {
            IContainer container = ContainerHelper.Create(new ConfigurationStub(), null);
            IAuthTokenGenerator authTokenGenerator = container.Resolve<IAuthTokenGenerator>();
            await authTokenGenerator.GetAuthTokenAsync(userId: 9);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
