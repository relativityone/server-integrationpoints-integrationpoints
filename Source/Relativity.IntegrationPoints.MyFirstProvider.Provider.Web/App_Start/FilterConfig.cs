﻿using System.Web.Mvc;

namespace Relativity.IntegrationPoints.MyFirstProvider.Web
{
	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}