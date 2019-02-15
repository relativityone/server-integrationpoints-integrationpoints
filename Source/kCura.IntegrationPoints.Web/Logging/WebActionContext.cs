﻿using System;

namespace kCura.IntegrationPoints.Web.Logging
{
	public class WebActionContext
	{
		public string ActionName { get; }

		public Guid ActionGuid { get; }

		public WebActionContext(string actionName, Guid actionGuid)
		{
			ActionName = actionName;
			ActionGuid = actionGuid;
		}
	}
}
