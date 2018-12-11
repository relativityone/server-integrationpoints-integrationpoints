using System;
using System.Net;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Services;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public abstract class BaseController : Controller
	{
		public ISessionService SessionService { get; set; }

		public Core.Services.CustomPageErrorService CreateError { get; set; }

		public GridModelFactory ModelFactory { get; set; }
		
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
