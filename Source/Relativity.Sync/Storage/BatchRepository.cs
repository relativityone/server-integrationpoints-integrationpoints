using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
    internal sealed class BatchRepository : IBatchRepository
    {
        private readonly IRdoManager _rdoManager;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IDateTime _dateTime;

        public BatchRepository(IRdoManager rdoManager,ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IDateTime dateTime)
        {
            _rdoManager = rdoManager;
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _dateTime = dateTime;
        }

        public Task<IBatch> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId, int totalDocumentsCount, int startingIndex)
        {
            return Batch.CreateAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, syncConfigurationArtifactId, exportRunId, totalDocumentsCount, startingIndex);
        }

        public Task<IBatch> GetAsync(int workspaceArtifactId, int artifactId)
        {
            return Batch.GetAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, artifactId);
        }

        public Task<IEnumerable<IBatch>> GetAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId)
        {
            return Batch.GetAllAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, syncConfigurationArtifactId, exportRunId);
        }

        public async Task DeleteAllForConfigurationAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
        {
            using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var request = new MassDeleteByCriteriaRequest
                {
                    ObjectIdentificationCriteria = new ObjectIdentificationCriteria()
                    {
                        ObjectType = new ObjectTypeRef
                        {
                            Guid = Batch.BatchObjectTypeGuid
                        },
                        Condition = $"'{Batch._PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}"
                    }
                };
                await objectManager.DeleteAsync(workspaceArtifactId, request).ConfigureAwait(false);
            }
        }

        public Task<IBatch> GetLastAsync(int workspaceArtifactId, int syncConfigurationId, Guid exportRunId)
        {
            return Batch.GetLastAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, syncConfigurationId, exportRunId);
        }

        public Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int workspaceArtifactId, int syncConfigurationId, Guid exportRunId)
        {
            return Batch.GetAllBatchesIdsToExecuteAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, syncConfigurationId, exportRunId);
        }

        public Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(int workspaceArtifactId, int syncConfigurationId, Guid exportRunId)
        {
            return Batch.GetAllSuccessfullyExecutedBatchesAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, syncConfigurationId, exportRunId);
        }

        public Task<IBatch> GetNextAsync(int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId, int startingIndex)
        {
            return Batch.GetNextAsync(_rdoManager, _serviceFactoryForAdmin, workspaceArtifactId, syncConfigurationArtifactId, startingIndex, exportRunId);
        }
        
        private static async Task<IEnumerable<int>> GetConfigurationsOlderThanAsync(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IDateTime dateTime, int workspaceArtifactId, TimeSpan olderThan)
        {
            DateTime createdBeforeDate = dateTime.UtcNow - olderThan;
            DateTime GetObjectCreationDate(RelativityObject relativityObject) => (DateTime)relativityObject.FieldValues.Single().Value;

            using (IObjectManager objectManager = await serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        Guid = new Guid(SyncRdoGuids.SyncConfigurationGuid)
                    },
                    Fields = new[]
                    {
                        new FieldRef()
                        {
                            Name = "System Created On"
                        }
                    },
                    IncludeNameInQueryResult = true
                };

                QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, request, 0, int.MaxValue).ConfigureAwait(false);
                IEnumerable<RelativityObject> oldConfigurations = queryResult.Objects.Where(configurationObject =>
                    GetObjectCreationDate(configurationObject) < createdBeforeDate);
                return oldConfigurations.Select(x => x.ArtifactID);
            }
        }
    }
}