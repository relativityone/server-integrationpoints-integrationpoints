using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public abstract class BaseController : Controller
	{
		public ISessionService SessionService { get; set; }
		public Core.Services.CreateError CreateError { get; set; }

		protected void LogException(Exception e, string controller = null, string action = null)
		{
			var message = String.Format("{0}/{1}", controller, action);
			var errorModel = new Core.Models.ErrorModel(SessionService.WorkspaceID, message, e);
			CreateError.Log(errorModel);
		}

		protected ActionResult RedirectToBase(string url)
		{
			//:( we have to do this since we are in an iframe :(
			return Content("<html><script>window.top.location.href = '" + url + "'; </script></html>");
		}

	}
}
