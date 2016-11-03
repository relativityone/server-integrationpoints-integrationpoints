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
		private readonly IErrorFactory _factory;
		private readonly ISystemEventLoggingService _systemEventLoggingService;

		/// <summary>
		/// For testing purposes only
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="systemEventLoggingService"></param>
		internal WebAPIFilterException(IErrorFactory factory, ISystemEventLoggingService systemEventLoggingService)
		{
			_factory = factory;
			_systemEventLoggingService = systemEventLoggingService;
		}

		public WebAPIFilterException(IErrorFactory factory)
		{
			_factory = factory;
			_systemEventLoggingService = new SystemEventLoggingService();
		}

		public override void Log(ExceptionLoggerContext context)
		{
			IErrorService errorService = null;
			try
			{
				var workspaceId = RetrieveWorkspaceId(context);
				var errorModel = CreateErrorModel(context, workspaceId);
				errorService = _factory.GetErrorService();
				errorService.Log(errorModel);
			}
			catch (Exception e)
			{
				var aggregateException = new AggregateException(e, context.Exception);
				_systemEventLoggingService.WriteErrorEvent("Integration Points", nameof(WebAPIFilterException), aggregateException);
			}
			finally
			{
				if ((_factory != null) && (errorService != null))
				{
					_factory.Release(errorService);
				}
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
			var exp = context.Exception;
			var errorModel = new ErrorModel(workspaceId, exp.Message, exp);
			return errorModel;
		}
	}
}