using System;
using System.Threading.Tasks;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation
{
    internal class ObjectManagerFacade : IObjectManagerFacade
    {
        private bool _isDisposed;

        private readonly Lazy<IObjectManager> _objectManager;

        public ObjectManagerFacade(Func<IObjectManager> objectManager)
        {
            _objectManager = new Lazy<IObjectManager>(objectManager);
        }

        public Task<CreateResult> CreateAsync(
            int workspaceArtifactID, 
            CreateRequest createRequest)
        {
            return _objectManager.Value.CreateAsync(
                    workspaceArtifactID,
                    createRequest);
        }

        public Task<ReadResult> ReadAsync(
            int workspaceArtifactID, 
            ReadRequest request)
        {
            return _objectManager.Value.ReadAsync(
                    workspaceArtifactID,
                    request);
        }

        public Task<UpdateResult> UpdateAsync(
            int workspaceArtifactID, 
            UpdateRequest request)
        {
            return _objectManager.Value.UpdateAsync(
                    workspaceArtifactID,
                    request);
        }

        public Task<MassUpdateResult> UpdateAsync(
            int workspaceArtifactID, 
            MassUpdateByObjectIdentifiersRequest request,
            MassUpdateOptions updateOptions)
        {
            return _objectManager.Value.UpdateAsync(
                workspaceArtifactID,
                request,
                updateOptions);
        }

        public Task<DeleteResult> DeleteAsync(
            int workspaceArtifactID, 
            DeleteRequest request)
        {
            return _objectManager.Value.DeleteAsync(
                    workspaceArtifactID,
                    request);
        }

        public Task<MassDeleteResult> DeleteAsync(
            int workspaceArtifactID, 
            MassDeleteByObjectIdentifiersRequest request)
        {
            return _objectManager.Value.DeleteAsync(
                workspaceArtifactID,
                request);
        }

        public Task<QueryResult> QueryAsync(
            int workspaceArtifactID, 
            QueryRequest request, 
            int start, 
            int length)
        {
            return _objectManager.Value.QueryAsync(
                    workspaceArtifactID,
                    request,
                    start,
                    length);
        }

        public Task<IKeplerStream> StreamLongTextAsync(
            int workspaceArtifactID, 
            RelativityObjectRef exportObject, 
            FieldRef longTextField)
        {
            return _objectManager.Value.StreamLongTextAsync(
                workspaceArtifactID,
                exportObject,
                longTextField);
        }

        public Task<ExportInitializationResults> InitializeExportAsync(
            int workspaceArtifactID, 
            QueryRequest queryRequest, 
            int start)
        {
            return _objectManager.Value.InitializeExportAsync(
                workspaceArtifactID,
                queryRequest,
                start);
        }

        public Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(
            int workspaceArtifactID, 
            Guid runID,
            int resultsBlockSize,
            int exportIndexID)
        {
            return _objectManager.Value.RetrieveResultsBlockFromExportAsync(
                workspaceArtifactID,
                runID,
                resultsBlockSize,
                exportIndexID);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            if (disposing && _objectManager.IsValueCreated)
            {
                _objectManager.Value.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
