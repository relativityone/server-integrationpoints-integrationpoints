using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;

namespace kCura.IntegrationPoints.Web.Extensions
{
    internal static class HttpConfigurationExtensions
    {
        public static void AddWebAPIFiltersProvider(this HttpConfiguration config, IFilterProvider filterProvider)
        {
            config.Services.Add(typeof(IFilterProvider), filterProvider);
        }

        public static void AddExceptionLogger(this HttpConfiguration config, IExceptionLogger exceptionLogger)
        {
            config.Services.Add(typeof(IExceptionLogger), exceptionLogger);
        }

        public static void AddMessageHandler(this HttpConfiguration config, DelegatingHandler handler)
        {
            config.MessageHandlers.Add(handler);
        }
    }
}