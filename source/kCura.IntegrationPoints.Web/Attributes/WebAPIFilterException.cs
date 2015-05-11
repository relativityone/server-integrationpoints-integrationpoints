using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Core.Models;

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
			var workspaceID = context.Request.GetRouteData().Values["workspaceID"] as string;
			var workspace = 0;
			int.TryParse(workspaceID, out workspace);
			var exp = context.Exception;
			var creator = _factory.GetErrorService();
			creator.Log(new ErrorModel(workspace, exp.Message, exp));
			_factory.Release(creator);

		}
	}
}