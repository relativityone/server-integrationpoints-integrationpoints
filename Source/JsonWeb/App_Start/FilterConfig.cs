using System.Web.Mvc;

namespace JsonWeb
{
	public class HandleError : HandleErrorAttribute
	{
		public override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
		}
	}

	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleError());
		}
	}
}