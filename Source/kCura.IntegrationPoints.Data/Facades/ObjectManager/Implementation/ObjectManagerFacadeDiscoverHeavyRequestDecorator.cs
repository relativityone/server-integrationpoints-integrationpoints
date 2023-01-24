using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.DTOs;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation
{
    using LogParameters = ValueTuple<string, object[]>;

    internal class ObjectManagerFacadeDiscoverHeavyRequestDecorator : IObjectManagerFacade
    {
        private const int _MAX_COUNT_OF_COLLECTION_IN_REQUEST = 100000;
        private const string _UNKNOWN = "[UNKNOWN]";

        private readonly IObjectManagerFacade _objectManager;
        private readonly IAPILog _logger;

        private bool _disposedValue;

        /// <summary>
        /// Discovers heavy requests sent to Object Manager and logs them
        /// </summary>
        /// <param name="objectManager">This object will be disposed when Dispose is called</param>
        /// <param name="logger">Logger where discovery results will be sent to</param>
        public ObjectManagerFacadeDiscoverHeavyRequestDecorator(
            IObjectManagerFacade objectManager,
            IAPILog logger)
        {
            _objectManager = objectManager;
            _logger = logger.ForContext<ObjectManagerFacadeDiscoverHeavyRequestDecorator>();
        }

        public Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest request)
        {
            Func<LogParameters> getWarningMessageHeader =
                () => GetWarningMessageHeader<CreateRequest>(
                    workspaceArtifactID,
                    rdoArtifactId: _UNKNOWN,
                    rdoType: request.ObjectType.Name);

            IEnumerable<FieldValueMap> fieldValues = request.FieldValues
                .Select(x => new FieldValueMap(x));

            AnalyzeFields(fieldValues, getWarningMessageHeader);

            return _objectManager.CreateAsync(workspaceArtifactID, request);
        }

        public Task<MassCreateResult> CreateAsync(int workspaceArtifactID, MassCreateRequest createRequest)
        {
            Func<LogParameters> getWarningMessageHeader =
                () => GetWarningMessageHeader<MassCreateRequest>(
                    workspaceArtifactID,
                    rdoArtifactId: _UNKNOWN,
                    rdoType: createRequest.ObjectType.Name);

            IEnumerable<FieldValueMap> fieldValues = createRequest.Fields
                .Select(x => new FieldValueMap(
                    new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = x.ArtifactID,
                        Guid = x.Guid,
                        Name = x.Name,
                        ViewFieldID = x.ViewFieldID
                    }
                }));

            AnalyzeFields(fieldValues, getWarningMessageHeader);

            return _objectManager.CreateAsync(workspaceArtifactID, createRequest);
        }

        public async Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
        {
            Func<LogParameters> getWarningMessageHeader =
                () => GetWarningMessageHeader<ReadRequest>(
                    workspaceArtifactID,
                    request.Object.ArtifactID.ToString(),
                    rdoType: _UNKNOWN);

            ReadResult result = await _objectManager
                .ReadAsync(workspaceArtifactID, request)
                .ConfigureAwait(false);

            IEnumerable<FieldValueMap> fieldValues = result.Object.FieldValues
                .Select(x => new FieldValueMap(x));

            AnalyzeFields(fieldValues, getWarningMessageHeader);

            return result;
        }

        public Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
        {
            Func<LogParameters> getWarningMessageHeader =
                () => GetWarningMessageHeader<UpdateRequest>(
                    workspaceArtifactID,
                    request.Object.ArtifactID.ToString(),
                    rdoType: _UNKNOWN);

            IEnumerable<FieldValueMap> fieldValues = request.FieldValues
                .Select(x => new FieldValueMap(x));

            AnalyzeFields(fieldValues, getWarningMessageHeader);

            return _objectManager.UpdateAsync(workspaceArtifactID, request);
        }

        public Task<MassUpdateResult> UpdateAsync(
            int workspaceArtifactID,
            MassUpdateByObjectIdentifiersRequest request,
            MassUpdateOptions updateOptions)
        {
            Func<LogParameters> getWarningMessage =
                () => GetWarningMessageHeader<UpdateRequest>(
                    workspaceArtifactID,
                    rdoArtifactId: _UNKNOWN,
                    rdoType: _UNKNOWN);

            AnalyzeMassUpdateObjectsCollection(getWarningMessage, request);
            AnalyzeMassUpdateFields(getWarningMessage, workspaceArtifactID, request);

            return _objectManager.UpdateAsync(workspaceArtifactID, request, updateOptions);
        }

        public Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
        {
            return _objectManager.DeleteAsync(workspaceArtifactID, request);
        }

        public Task<MassDeleteResult> DeleteAsync(int workspaceArtifactID, MassDeleteByObjectIdentifiersRequest request)
        {
            Func<LogParameters> getWarningMessage =
                () => GetWarningMessageHeader<MassDeleteByObjectIdentifiersRequest>(
                    workspaceArtifactID,
                    rdoArtifactId: _UNKNOWN,
                    rdoType: _UNKNOWN);

            AnalyzeMassDeleteObjectsCollection(getWarningMessage, request);

            return _objectManager.DeleteAsync(workspaceArtifactID, request);
        }

        public async Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
        {
            Func<LogParameters> getWarningMessageHeader =
                () => GetWarningMessageHeader<QueryRequest>(
                    workspaceArtifactID,
                    rdoArtifactId: _UNKNOWN,
                    rdoType: request.ObjectType.Name);

            QueryResult result = await _objectManager
                .QueryAsync(workspaceArtifactID, request, start, length)
                .ConfigureAwait(false);

            IEnumerable<FieldValueMap> fieldValues = result.Objects
                .SelectMany(x => x.FieldValues)
                .Select(x => new FieldValueMap(x));

            AnalyzeFields(fieldValues, getWarningMessageHeader);

            return result;
        }

        public async Task<QueryResultSlim> QuerySlimAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
        {
            Func<LogParameters> getWarningMessageHeader =
                () => GetWarningMessageHeader<QueryRequest>(
                    workspaceArtifactID,
                    rdoArtifactId: _UNKNOWN,
                    rdoType: request.ObjectType.Name);

            QueryResultSlim result = await _objectManager
                .QuerySlimAsync(workspaceArtifactID, request, start, length)
                .ConfigureAwait(false);

            List<FieldValuePair> fieldValuePairs = new List<FieldValuePair>();

            for (int i = 0; i < result.Objects.Count; i++)
            {
                var fieldValuePair = new FieldValuePair
                {
                    Field = result.Fields[i],
                    Value = result.Objects[i].Values[0]
                };
                fieldValuePairs.Add(fieldValuePair);
            }

            IEnumerable<FieldValueMap> fieldValues = fieldValuePairs
                .Select(x => new FieldValueMap(x));

            AnalyzeFields(fieldValues, getWarningMessageHeader);

            return result;
        }

        public Task<IKeplerStream> StreamLongTextAsync(int workspaceArtifactID, RelativityObjectRef exportObject, FieldRef longTextField)
        {
            return _objectManager.StreamLongTextAsync(workspaceArtifactID, exportObject, longTextField);
        }

        public Task<ExportInitializationResults> InitializeExportAsync(
            int workspaceArtifactID,
            QueryRequest queryRequest,
            int start)
        {
            return _objectManager.InitializeExportAsync(workspaceArtifactID, queryRequest, start);
        }

        public Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(
            int workspaceArtifactID,
            Guid runID,
            int resultsBlockSize,
            int exportIndexID)
        {
            return _objectManager.RetrieveResultsBlockFromExportAsync(
                workspaceArtifactID,
                runID,
                resultsBlockSize,
                exportIndexID);
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

        private void AnalyzeMassUpdateObjectsCollection(
            Func<LogParameters> getWarningMessageHeader,
            MassUpdateByObjectIdentifiersRequest request)
        {
            if (request.Objects.Count > _MAX_COUNT_OF_COLLECTION_IN_REQUEST)
            {
                LogParameters massUpdateWarningMessage = ("Requested mass update operation exceeded max collection count - {objectsCount}, when allowed is {maxCollectionCount}",
                    new object[] { request.Objects.Count, _MAX_COUNT_OF_COLLECTION_IN_REQUEST });

                LogParameters[] warningsToLog = { getWarningMessageHeader(), massUpdateWarningMessage };
                LogWarnings(warningsToLog, new StackTrace());
            }
        }

        private void AnalyzeMassDeleteObjectsCollection(Func<LogParameters> getWarningMessageHeader, MassDeleteByObjectIdentifiersRequest request)
        {
            if (request.Objects.Count > _MAX_COUNT_OF_COLLECTION_IN_REQUEST)
            {
                LogParameters massDeleteWarningMessage = ("Requested mass delete operation exceeded max collection count - {objectsCount}, when allowed is {maxCollectionCount}",
                    new object[] { request.Objects.Count, _MAX_COUNT_OF_COLLECTION_IN_REQUEST });

                LogParameters[] warningsToLog = { getWarningMessageHeader(), massDeleteWarningMessage };
                LogWarnings(warningsToLog, new StackTrace());
            }
        }

        private void AnalyzeMassUpdateFields(
            Func<LogParameters> getWarningMessageHeader,
            int workspaceArtifactID,
            MassUpdateByObjectIdentifiersRequest request)
        {
            IEnumerable<FieldValueMap> fieldValues = request.FieldValues
                .Select(x => new FieldValueMap(x));

            AnalyzeFields(fieldValues, getWarningMessageHeader);
        }

        private void AnalyzeFields(
            IEnumerable<FieldValueMap> fieldValues,
            Func<LogParameters> getWarningMessageHeader)
        {
            IList<LogParameters> warnings = DiscoverFieldsCollectionsWhichExceedMaxCountValue(fieldValues);

            if (!warnings.Any())
            {
                return;
            }

            warnings.Insert(0, getWarningMessageHeader());
            LogWarnings(warnings, new StackTrace());
        }

        private IList<LogParameters> DiscoverFieldsCollectionsWhichExceedMaxCountValue(
            IEnumerable<FieldValueMap> fieldValues)
        {
            return fieldValues
                .Select(fieldValue => new
                {
                    FieldValue = fieldValue,
                    Value = fieldValue.Value as ICollection
                })
                .Where(x => x.Value != null && x.Value.Count > _MAX_COUNT_OF_COLLECTION_IN_REQUEST)
                .Select(x =>
                (
                    "Requested field {fieldName} exceeded max collection count - {count}, when allowed is {maxAllowedCount}",
                    new object[] { x.FieldValue.FieldName, x.Value.Count, _MAX_COUNT_OF_COLLECTION_IN_REQUEST }))
                .ToList();
        }

        private void LogWarnings(IList<LogParameters> warnings, StackTrace stackTrace)
        {
            Exception exception = new Exception("This exception has been logged only to provide stack trace, no actual exception occurred").SetStackTrace(stackTrace);

            foreach ((string messageTemplate, object[] parameters) in warnings)
            {
                _logger.LogWarning(exception, messageTemplate, parameters);
            }
        }

        private LogParameters GetWarningMessageHeader<T>(
            int workspaceArtifactId,
            string rdoArtifactId,
            string rdoType)
        {
            string operationName = GetOperationNameForRequestType<T>();
            return ("Heavy request discovered when executing {operationName} on object of type [{rdoType}], id {rdoArtifactId} with ObjectManager (Workspace: {workspaceArtifactId}).",
                new object[] { operationName, rdoType, rdoArtifactId, workspaceArtifactId });
        }

        private string GetOperationNameForRequestType<T>()
        {
            return typeof(T).Name.Replace("Request", string.Empty).ToUpperInvariant();
        }
    }
}
