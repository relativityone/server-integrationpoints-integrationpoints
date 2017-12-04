using System;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Logging;

namespace kCura.IntegrationPoints.Web.Attributes
{
	//we don't use Raygun but the article is good
	//http://www.strathweb.com/2014/03/asp-net-web-api-exception-logging-raygun-io/
	public class WebAPIFilterException : ExceptionLogger
	{
		private readonly IErrorService _service;
		private readonly ISystemEventLoggingService _systemEventLoggingService;

		/// <summary>
		/// For testing purposes only
		/// </summary>
		/// <param name="service"></param>
		/// <param name="systemEventLoggingService"></param>
		internal WebAPIFilterException(IErrorService service, ISystemEventLoggingService systemEventLoggingService)
		{
			_service = service;
			_systemEventLoggingService = systemEventLoggingService;
		}

		public WebAPIFilterException(IErrorService service)
		{
			_service = service;
			_systemEventLoggingService = new SystemEventLoggingService();
		}

		public override void Log(ExceptionLoggerContext context)
		{
			try
			{
				var workspaceId = RetrieveWorkspaceId(context);
				var errorModel = CreateErrorModel(context, workspaceId);
				_service.Log(errorModel);
			}
			catch (Exception e)
			{
				var aggregateException = new AggregateException(e, context.Exception);
				_systemEventLoggingService.WriteErrorEvent("Integration Points", nameof(WebAPIFilterException), aggregateException);
			}
		}

		private static int RetrieveWorkspaceId(ExceptionLoggerContext context)
		{
			var workspaceUrlParameter = context.Request.GetRouteData().Values["workspaceID"] as string;
			int workspaceId;
			int.TryParse(workspaceUrlParameter, out workspaceId);
			return workspaceId;
		}

		private static ErrorModel CreateErrorModel(ExceptionLoggerContext context, int workspaceId)
		{
			return new ErrorModel(context.Exception, true)
			{
				WorkspaceId = workspaceId,
				Location = context.Request.RequestUri.PathAndQuery,
			};
		}
	}
}