using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Extensions
{
	internal static class HttpConfigurationExtensions
	{
		public static void RegisterWebAPIFilters(this HttpConfiguration config)
		{
			config.Filters.Add(new LogApiExceptionFilterAttribute());
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