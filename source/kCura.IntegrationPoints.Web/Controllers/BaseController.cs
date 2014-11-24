using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public abstract class BaseController : Controller
	{
		public ISessionService SessionService { get; set; }

		public Core.Services.CreateError CreateError { get; set; }

		protected void LogException(Exception e, string controller = null, string action = null)
		{
			if (string.IsNullOrEmpty(controller))
			{
				controller = base.RouteData.Values["controller"] as string;
			}
			if (string.IsNullOrEmpty(action))
			{
				action = base.RouteData.Values["action"] as string;
			}
			var message = string.Format("{0}/{1}", controller, action);
			var errorModel = new Core.Models.ErrorModel(SessionService.WorkspaceID, message, e);
			CreateError.Log(errorModel);
		}

		protected ActionResult RedirectToBase(string url)
		{
			//:( we have to do this since we are in an iframe :(
			return Content("<html><script>window.top.location.href = '" + url + "'; </script></html>");
		}

		public JsonNetResult JsonNetResult(object data, HttpStatusCode code = HttpStatusCode.OK)
		{
			return new JsonNetResult().GetJsonNetResult(data, (int)code);
		}

	}
}
