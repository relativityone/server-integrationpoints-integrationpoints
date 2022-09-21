using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation
{
    internal class ObjectManagerFacadeInstrumentationDecorator : IObjectManagerFacade
    {
        private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
        private readonly IAPILog _logger;
        private readonly IObjectManagerFacade _objectManager;

        private bool _isDisposed;

        public ObjectManagerFacadeInstrumentationDecorator(
            IObjectManagerFacade objectManager,
            IExternalServiceInstrumentationProvider instrumentationProvider,
            IAPILog logger)
        {
            _objectManager = objectManager;
            _instrumentationProvider = instrumentationProvider;
            _logger = logger.ForContext<ObjectManagerFacadeInstrumentationDecorator>();
        }

        public async Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest)
        {
            IExternalServiceInstrumentationStarted instrumentation = StartInstrumentation();
            CreateResult result = await ExecuteAsync(
                    x => x.CreateAsync(workspaceArtifactID, createRequest),
                    instrumentation)
                .ConfigureAwait(false);
            CompleteResultWithEventHandlers(result.EventHandlerStatuses, instrumentation);
            return result;
        }

        public async Task<MassCreateResult> CreateAsync(int workspaceArtifactID, MassCreateRequest createRequest)
        {
            IExternalServiceInstrumentationStarted instrumentation = StartInstrumentation();
            MassCreateResult result = await ExecuteAsync(
                    x => x.CreateAsync(workspaceArtifactID, createRequest),
                    instrumentation)
                .ConfigureAwait(false);

            if (result.Success)
            {
                instrumentation.Completed();
            }
            else
            {
                instrumentation.Failed(result.Message);
            }

            return result;
        }

        public Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
        {
            return ExecuteAsync(
                x => x.DeleteAsync(workspaceArtifactID, request));
        }

        public async Task<MassDeleteResult> DeleteAsync(int workspaceArtifactID, MassDeleteByObjectIdentifiersRequest request)
        {
            MassDeleteResult result = await ExecuteAsync(
                    x => x.DeleteAsync(workspaceArtifactID, request))
                .ConfigureAwait(false);
            return result;
        }

        public Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
        {
            return ExecuteAsync(
                x => x.QueryAsync(workspaceArtifactID, request, start, length));
        }

        public Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
        {
            return ExecuteAsync(
                x => x.ReadAsync(workspaceArtifactID, request));
        }

        public async Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
        {
            IExternalServiceInstrumentationStarted instrumentation = StartInstrumentation();
            UpdateResult result = await ExecuteAsync(
                    x => x.UpdateAsync(workspaceArtifactID, request),
                    instrumentation)
                .ConfigureAwait(false);
            CompleteResultWithEventHandlers(result.EventHandlerStatuses, instrumentation);
            return result;
        }

        public async Task<MassUpdateResult> UpdateAsync(
            int workspaceArtifactID,
            MassUpdateByObjectIdentifiersRequest request,
            MassUpdateOptions updateOptions)
        {
            IExternalServiceInstrumentationStarted instrumentation = StartInstrumentation();
            MassUpdateResult result = await ExecuteAsync(
                    x => x.UpdateAsync(workspaceArtifactID, request, updateOptions),
                    instrumentation)
                .ConfigureAwait(false);

            if (result.Success)
            {
                instrumentation.Completed();
            }
            else
            {
                instrumentation.Failed(result.Message);
            }

            return result;
        }

        public Task<IKeplerStream> StreamLongTextAsync(int workspaceArtifactID, RelativityObjectRef exportObject, FieldRef longTextField)
        {
            return ExecuteAsync(
                x => x.StreamLongTextAsync(workspaceArtifactID, exportObject, longTextField));
        }

        public Task<ExportInitializationResults> InitializeExportAsync(int workspaceArtifactID, QueryRequest queryRequest, int start)
        {
            return ExecuteAsync(
                x => x.InitializeExportAsync(workspaceArtifactID, queryRequest, start));
        }

        public Task<RelativityObjectSlim[]> RetrieveResultsBlockFromExportAsync(
            int workspaceArtifactID,
            Guid runID,
            int resultsBlockSize,
            int exportIndexID)
        {
            return ExecuteAsync(
                x => x.RetrieveResultsBlockFromExportAsync(workspaceArtifactID, runID, resultsBlockSize, exportIndexID));
        }

        #region IDisposable Support

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _objectManager?.Dispose();
            }

            _isDisposed = true;
        }

        #endregion

        private void CompleteResultWithEventHandlers(List<EventHandlerStatus> eventHandlerStatuses, IExternalServiceInstrumentationStarted startedInstrumentation)
        {
            string[] failedEventHandlersMessages =
                eventHandlerStatuses.Where(x => !x.Success).Select(x => x.Message).ToArray();
            if (failedEventHandlersMessages.Any())
            {
                string reason = string.Join(";", failedEventHandlersMessages);
                startedInstrumentation.Failed(reason);
            }
            else
            {
                startedInstrumentation.Completed();
            }
        }

        private Task<T> ExecuteAsync<T>(
            Func<IObjectManagerFacade, Task<T>> function,
            IExternalServiceInstrumentationStarted instrumentation,
            [CallerMemberName] string operation = "")
        {
            return ExecuteAsync(function, instrumentation, false, operation);
        }

        private Task<T> ExecuteAsync<T>(
            Func<IObjectManagerFacade, Task<T>> function,
            [CallerMemberName] string operation = "")
        {
            IExternalServiceInstrumentationStarted instrumentation = StartInstrumentation(operation);
            return ExecuteAsync(function, instrumentation, true, operation);
        }

        private async Task<T> ExecuteAsync<T>(
            Func<IObjectManagerFacade, Task<T>> function,
            IExternalServiceInstrumentationStarted instrumentation,
            bool completeInstrumentationOnSuccess,
            string operation)
        {
            try
            {
                T result = await function(_objectManager).ConfigureAwait(false);
                if (completeInstrumentationOnSuccess)
                {
                    instrumentation.Completed();
                }

                return result;
            }
            catch (ServiceNotFoundException ex)
            {
                instrumentation.Failed(ex);
                throw LogServiceNotFoundException(operation, ex);
            }
            catch (Exception ex)
            {
                instrumentation.Failed(ex);
                throw;
            }
        }

        private IExternalServiceInstrumentationStarted StartInstrumentation([CallerMemberName] string operationName = "")
        {
            IExternalServiceInstrumentation instrumentation =
                _instrumentationProvider.Create(ExternalServiceTypes.KEPLER, nameof(IObjectManager), operationName);
            return instrumentation.Started();
        }

        private IntegrationPointsException LogServiceNotFoundException(string operationName, ServiceNotFoundException ex)
        {
            string message =
                $"Error while connecting to object manager service. Cannot perform {operationName} operation.";
            _logger.LogError(
                "Error while connecting to object manager service. Cannot perform {operationName} operation.",
                operationName);

            return new IntegrationPointsException(message, ex)
            {
                ShouldAddToErrorsTab = true,

                Source = IntegrationPointsExceptionSource.KEPLER
            };
        }
    }
}
