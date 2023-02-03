using System;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Helpers.Logging;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers
{
    /// <summary>
    /// Represents the base class for Post Install event handlers.
    /// </summary>
    public abstract class PostInstallEventHandlerBase : PostInstallEventHandler
    {
        private const string _ACTION_NAME = "Install";

        private readonly Lazy<IErrorService> _errorService;
        private readonly Lazy<IAPILog> _log;
        private readonly Lazy<IRelativityObjectManager> _objectManager;
        private readonly Lazy<IRelativityObjectManagerFactory> _objectManagerFactory;

        protected abstract string SuccessMessage { get; }
        protected abstract string GetFailureMessage(Exception ex);

        protected IAPILog Logger => _log.Value;

        protected IRelativityObjectManager ObjectManager => _objectManager.Value;
        protected IRelativityObjectManagerFactory ObjectManagerFactory => _objectManagerFactory.Value;

        protected PostInstallEventHandlerBase()
        {
            _log = new Lazy<IAPILog>(CreateLogger);
            _objectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(CreateObjectManagerFactory);
            _objectManager = new Lazy<IRelativityObjectManager>(CreateObjectManager);

            _errorService = new Lazy<IErrorService>(() =>
                new EhErrorService(new CreateErrorRdoQuery(Helper.GetServicesManager(), Logger), Logger));
        }

        protected virtual IAPILog CreateLogger()
        {
            return Helper.GetLoggerFactory().GetLogger().ForContext<PostInstallEventHandlerBase>();
        }

        protected abstract void Run();

        /// <summary>
        /// Executes method that is called after an application is installed.
        /// </summary>
        /// <returns></returns>
        public sealed override Response Execute()
        {
            var response = new Response
            {
                Message = SuccessMessage,
                Success = true
            };
            EhCorrelationContext correlationContext = CreateCorrelationContext();
            using (Logger.LogContextPushProperties(correlationContext))
            {
                try
                {
                    LogExecutionInfo();

                    OnRaisePostInstallPreExecuteEvent();
                    Run();
                    return response;

                }
                catch (Exception ex)
                {
                    response = HandleError(correlationContext.WorkspaceId.GetValueOrDefault(), ex, correlationContext.CorrelationId);
                }
                finally
                {
                    OnRaisePostInstallPostExecuteEvent(response);
                }
                return response;
            }
        }

        protected virtual void OnRaisePostInstallPreExecuteEvent()
        {
        }

        protected virtual void OnRaisePostInstallPostExecuteEvent(Response response)
        {
        }

        private IRelativityObjectManager CreateObjectManager()
        {
            return ObjectManagerFactory.CreateRelativityObjectManager(Helper.GetActiveCaseID());
        }

        private IRelativityObjectManagerFactory CreateObjectManagerFactory()
        {
            return new RelativityObjectManagerFactory(Helper);
        }

        private EhCorrelationContext CreateCorrelationContext()
        {
            Guid ehGuid = GetEventHandlerGuid();

            return new EhCorrelationContext
            {
                ActionName = _ACTION_NAME,
                CorrelationId = Guid.NewGuid(),
                InstallerGuid = ehGuid,
                WorkspaceId = LogHelper.GetValueAndLogEx(() => Helper.GetActiveCaseID(), $"Cannot extract Workspace Id in {ehGuid} installer", Logger)
            };
        }

        private Response HandleError(int workspaceID, Exception ex, Guid correlationId)
        {
            string descError = GetFailureMessage(ex);

            var errorModel = new ErrorModel(ex, true/* We want to add each errro to Error tab*/, descError)
            {
                WorkspaceId = workspaceID,
                CorrelationId = correlationId
            };
            _errorService.Value.Log(errorModel);

            return new Response
            {
                Exception = ex,
                Message = descError,
                Success = false
            };
        }

        private Guid GetEventHandlerGuid()
        {
            object[] type = GetType().GetCustomAttributes(typeof(GuidAttribute), true);

            GuidAttribute guid = type.OfType<GuidAttribute>().FirstOrDefault();
            return guid == null ? Guid.Empty : new Guid(guid.Value);
        }

        private void LogExecutionInfo()
        {
            Logger.LogInformation("Post install EventHandler started: {eventHandler} in workspace {workspaceId}", this.GetType().Name, Helper.GetActiveCaseID());
        }
    }
}