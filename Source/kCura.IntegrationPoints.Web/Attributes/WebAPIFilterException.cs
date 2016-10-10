using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Logging;

namespace kCura.IntegrationPoints.Web.Attributes
{


	//we don't use Raygun but the article is good
	//http://www.strathweb.com/2014/03/asp-net-web-api-exception-logging-raygun-io/
	public class WebAPIFilterException : ExceptionLogger
	{
		private readonly IErrorFactory _factory;
		public WebAPIFilterException(IErrorFactory factory)
		{
			_factory = factory;
		}

		public override void Log(ExceptionLoggerContext context)
		{
			try
			{
				var workspaceID = context.Request.GetRouteData().Values["workspaceID"] as string;
				var workspace = 0;
				int.TryParse(workspaceID, out workspace);
				var exp = context.Exception;
				var creator = _factory.GetErrorService();
				creator.Log(new ErrorModel(workspace, exp.Message, exp));
				_factory.Release(creator);
			}
			catch (Exception)
			{
				SystemEventLoggingService.WriteErrorEvent("Integration Points", "WebAPIFilterException", context.Exception);
			}
		}
	}
}