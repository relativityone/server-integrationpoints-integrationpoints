using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation
{
    internal class ObjectManagerFacadeRetryDecorator : IObjectManagerFacade
    {
        private const ushort _MAX_NUMBER_OF_RETRIES = 3;
        private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

        private readonly IRetryHandler _retryHandler;
        private readonly IObjectManagerFacade _objectManager;

        private bool _disposedValue;

        /// <summary>
        /// Creates new instance of object manager facade with implemented retries
        /// </summary>
        /// <param name="objectManager">This object will be disposed when Dispose is called</param>
        /// <param name="retryHandlerFactory"></param>
        public ObjectManagerFacadeRetryDecorator(IObjectManagerFacade objectManager, IRetryHandlerFactory retryHandlerFactory)
        {
            _objectManager = objectManager;
            _retryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RETRIES, _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC);
        }

        public Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                    () => _objectManager.CreateAsync(workspaceArtifactID, createRequest));
        }

        public Task<MassCreateResult> CreateAsync(int workspaceArtifactID, MassCreateRequest createRequest)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.CreateAsync(workspaceArtifactID, createRequest));
        }

        public Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                    () => _objectManager.DeleteAsync(workspaceArtifactID, request));
        }

        public Task<MassDeleteResult> DeleteAsync(int workspaceArtifactID, MassDeleteByObjectIdentifiersRequest request)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.DeleteAsync(workspaceArtifactID, request));
        }

        public Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.QueryAsync(
                    workspaceArtifactID,
                    request,
                    start,
                    length));
        }

        public Task<QueryResultSlim> QuerySlimAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.QuerySlimAsync(
                    workspaceArtifactID,
                    request,
                    start,
                    length));
        }

        public Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.ReadAsync(workspaceArtifactID, request));
        }

        public Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.UpdateAsync(workspaceArtifactID, request));
        }

        public Task<MassUpdateResult> UpdateAsync(
            int workspaceArtifactID,
            MassUpdateByObjectIdentifiersRequest request,
            MassUpdateOptions updateOptions)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.UpdateAsync(workspaceArtifactID, request, updateOptions));
        }

        public Task<IKeplerStream> StreamLongTextAsync(int workspaceArtifactID, RelativityObjectRef exportObject, FieldRef longTextField)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.StreamLongTextAsync(workspaceArtifactID, exportObject, longTextField));
        }

        public Task<ExportInitializationResults> InitializeExportAsync(int workspaceArtifactID, QueryRequest queryRequest, int start)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.InitializeExportAsync(workspaceArtifactID, queryRequest, start));
        }

        public Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(
            int workspaceArtifactID,
            Guid runID,
            int resultsBlockSize,
            int exportIndexID)
        {
            return _retryHandler.ExecuteWithRetriesAsync(
                () => _objectManager.RetrieveResultsBlockFromExportAsync(
                    workspaceArtifactID,
                    runID,
                    resultsBlockSize,
                    exportIndexID));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _objectManager?.Dispose();
            }

            _disposedValue = true;
        }
    }
}
