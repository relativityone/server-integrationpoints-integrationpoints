using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Web;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Web.Attributes
{
	public class WebAPIFilterException : ExceptionFilterAttribute
	{
		private readonly IErrorFactory _factory;
		public WebAPIFilterException(IErrorFactory factory)
		{
			_factory = factory;
		}
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			try
			{
				var worksspaceID = actionExecutedContext.Request.GetRouteData().Values["workspaceID"] as string;
				var workspsace = 0;
				int.TryParse(worksspaceID, out workspsace);
				var exp = actionExecutedContext.Exception;
				var creator = _factory.GetErrorService();
				creator.Log(new ErrorModel(workspsace, exp.Message, exp));
				_factory.Release(creator);
				if (exp.GetType() == typeof(AuthenticationException))
				{
					actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
					actionExecutedContext.Response.Content = new StringContent(exp.Message);
				}
				
			}
			catch
			{}
		}
	}
}