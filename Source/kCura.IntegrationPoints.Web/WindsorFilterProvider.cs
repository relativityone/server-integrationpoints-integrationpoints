using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Filters;

namespace kCura.IntegrationPoints.Web
{
    public class WindsorFilterProvider : IFilterProvider
    {
        private readonly Func<LogApiExceptionFilterAttribute, ExceptionFilter> _filterFactory;

        public WindsorFilterProvider(Func<LogApiExceptionFilterAttribute, ExceptionFilter> filterFactory)
        {
            _filterFactory = filterFactory;
        }

        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            foreach (LogApiExceptionFilterAttribute attribute in actionDescriptor.GetCustomAttributes<LogApiExceptionFilterAttribute>(inherit: false))
            {
                ExceptionFilter myFilter = _filterFactory(attribute);
                yield return new FilterInfo(myFilter, FilterScope.Action);
            }
        }
    }
}