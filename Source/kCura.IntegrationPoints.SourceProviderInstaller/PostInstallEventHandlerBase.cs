using System;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Helpers.Logging;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
#pragma warning disable 1591
	public abstract class PostInstallEventHandlerBase : PostInstallEventHandler
	{
		private const string _ACTION_NAME = "Install";

		private readonly Lazy<IErrorService> _errorService;

		private readonly Lazy<IAPILog> _log;

		protected abstract string SuccessMessage { get; }
		protected abstract string GetFailureMessage(Exception ex);

		protected IAPILog Logger => _log.Value;

		protected PostInstallEventHandlerBase()
		{
			_log = new Lazy<IAPILog>(CreateLogger);

			_errorService = new Lazy<IErrorService>(() => 
				new EhErrorService(new CreateErrorRdoQuery(new RsapiClientFactory(Helper), Logger, new SystemEventLoggingService()), Logger));
		}

		protected virtual IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<PostInstallEventHandlerBase>();
		}

		protected abstract void Run();

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

#pragma warning restore 1591