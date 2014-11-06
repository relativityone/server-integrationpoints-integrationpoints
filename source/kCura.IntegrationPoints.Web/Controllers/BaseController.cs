using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Controllers
{
	public abstract class BaseController : Controller
	{
		public const string SESSION_KEY = "__WEB_SESSION_KEY__";

		public ISessionService SessionService
		{
			get
			{
				var session = Session[SESSION_KEY] as ISessionService;
				if (session == null)
				{
					session = new SessionService();
					Session[SESSION_KEY] = session;
				}
				return session;
			}
		}
		
	}
}
