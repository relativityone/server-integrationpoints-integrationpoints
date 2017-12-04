using System;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Helpers.Logging;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Extensions;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public abstract class IntegrationPointMigrationEventHandlerBase : PostWorkspaceCreateEventHandlerBase
	{
		private const string _ACTION_NAME = "Post Workspace Create";
		private readonly Lazy<IErrorService> _errorService;
		private ICaseServiceContext _workspaceTemplateServiceContext;
		private IAPILog _logger;

		protected IntegrationPointMigrationEventHandlerBase()
		{
			_errorService = new Lazy<IErrorService>(() =>
				new EhErrorService(new CreateErrorRdoQuery(new RsapiClientFactory(Helper), Logger, new SystemEventLoggingService())));
		}

		private IAPILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = Helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationMigrationEventHandler>();
				}

				return _logger;
			}
		}

		protected ICaseServiceContext WorkspaceTemplateServiceContext
		{
			get
			{
				if (_workspaceTemplateServiceContext == null)
				{
					_workspaceTemplateServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, TemplateWorkspaceID);
				}
				return _workspaceTemplateServiceContext;
			}
		}

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
					return HandleError(correlationContext.WorkspaceId.GetValueOrDefault(), ex);
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
				CorrelationId = ehGuid,
				WorkspaceId = LogHelper.GetValueAndLogEx(() => Helper.GetActiveCaseID(), $"Cannot extract Workspace Id in {ehGuid} installer", Logger)
			};
		}

		private Response HandleError(int wkspId, Exception ex)
		{
			string descError = GetFailureMessage(ex);
			string errorMessage = "Source: EventHandler - {0}";

			_errorService.Value.Log(new ErrorModel()
				{
					Message = string.Format(errorMessage, descError),
					FullError = ex.FlattenErrorMessages(),
					WorkspaceId = wkspId
				}
			);
			Logger.LogError(ex, errorMessage, descError);
			return new Response
			{
				Exception = ex,
				Message = string.Format(errorMessage, descError),
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