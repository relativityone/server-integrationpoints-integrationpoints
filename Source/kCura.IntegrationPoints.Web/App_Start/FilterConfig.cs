﻿using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web
{
	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}