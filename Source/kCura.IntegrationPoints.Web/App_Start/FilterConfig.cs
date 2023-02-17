using System.Web.Http.Filters;
using System.Web.Mvc;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web
{
    public static class FilterConfig
    {
        public static void RegisterGlobalMvcFilters(GlobalFilterCollection mvcFilters)
        {
            mvcFilters.Add(new HandleErrorAttribute());
            mvcFilters.Add(new MvcActionExecutionTimeMetricsFilterAttribute());
        }

        public static void RegisterGlobalApiFilters(HttpFilterCollection apiFilters)
        {
            apiFilters.Add(new ApiActionExecutionTimeMetricsFilterAttribute());
        }
    }
}
