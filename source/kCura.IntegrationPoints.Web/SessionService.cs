using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Web
{
	public class SessionService : ISessionService
	{
		public const string SESSION_KEY = "__WEB_SESSION_KEY__";
		
		public static ISessionService Session
		{
			get
			{
				var session = HttpContext.Current.Session[SESSION_KEY] as ISessionService;
				if (session == null)
				{
					session = new SessionService();
					HttpContext.Current.Session[SESSION_KEY] = session;
				}
				return session;
			}
		}

		public SessionService()
		{
			Fields = new Dictionary<string, IEnumerable<FieldMap>>();
		}

		public int WorkspaceID
		{
			get { return (int) HttpContext.Current.Session["workspaceID"]; }
		}

		
		public Dictionary<string, IEnumerable<FieldMap>> Fields{ get; set; }
	}
}