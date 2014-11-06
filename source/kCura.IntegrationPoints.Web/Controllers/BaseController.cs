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
	}
}
