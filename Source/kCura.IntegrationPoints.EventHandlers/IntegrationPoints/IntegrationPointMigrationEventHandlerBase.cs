using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Helpers.Logging;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using Relativity.API;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class IntegrationPointMigrationEventHandlerBase : PostWorkspaceCreateEventHandlerBase
    {
        private ICaseServiceContext _workspaceTemplateServiceContext;
        private IAPILog _loggerValue;

        private const string _ACTION_NAME = "Post Workspace Create";
        private readonly Lazy<IErrorService> _errorService;

        protected IntegrationPointMigrationEventHandlerBase()
        {
            _errorService = new Lazy<IErrorService>(() =>
                new EhErrorService(new CreateErrorRdoQuery(Helper.GetServicesManager(), new SystemEventLoggingService(), Logger), Logger));
        }

        protected IntegrationPointMigrationEventHandlerBase(IErrorService errorService)
        {
            _errorService = new Lazy<IErrorService>(() => errorService);
        }

        protected IAPILog Logger =>
            _loggerValue
            ?? (_loggerValue = Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointMigrationEventHandlerBase>());

        protected ICaseServiceContext WorkspaceTemplateServiceContext =>
            _workspaceTemplateServiceContext
            ?? (_workspaceTemplateServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, TemplateWorkspaceID));

        public sealed override Response Execute()
        {
            EhCorrelationContext correlationContext = CreateCorrelationContext();
            using (Logger.LogContextPushProperties(correlationContext))
            {
                try
                {
                    Run();

                    return new Response
                    {
                        Message = SuccessMessage,
                        Success = true
                    };
                }
                catch (Exception ex)
                {
                    return HandleError(correlationContext.WorkspaceId.GetValueOrDefault(), ex, correlationContext.CorrelationId);
                }
            }
        }

        protected abstract void Run();

        protected abstract string SuccessMessage { get; }
        protected abstract string GetFailureMessage(Exception ex);

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

        private Response HandleError(int wkspId, Exception ex, Guid correlationId)
        {
            string descError = GetFailureMessage(ex);

            var errorModel = new ErrorModel(ex, true/* We want to add each errro to Error tab*/, descError)
            {
                WorkspaceId = wkspId,
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
    }
}