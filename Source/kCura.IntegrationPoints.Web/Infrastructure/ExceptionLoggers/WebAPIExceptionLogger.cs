using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Infrastructure.MessageHandlers;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Infrastructure.ExceptionLoggers
{
    //we don't use Raygun but the article is good
    //http://www.strathweb.com/2014/03/asp-net-web-api-exception-logging-raygun-io/
    public class WebAPIExceptionLogger : ExceptionLogger
    {
        private readonly Func<IErrorService> _errorServiceFactory;
        private readonly Func<IAPILog> _loggerFactory;

        /// <summary>
        /// For testing purposes only
        /// </summary>
        internal WebAPIExceptionLogger(IErrorService service, IAPILog logger)
        {
            _errorServiceFactory = () => service;
            _loggerFactory = () => logger;
        }

        public WebAPIExceptionLogger(Func<IErrorService> errorServiceFactory, Func<IAPILog> loggerFactory)
        {
            _errorServiceFactory = errorServiceFactory;
            _loggerFactory = loggerFactory;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            try
            {
                int workspaceId = RetrieveWorkspaceId(context);
                ErrorModel errorModel = CreateErrorModel(context, workspaceId);

                IErrorService errorService = _errorServiceFactory();
                errorService.Log(errorModel);
            }
            catch (Exception e)
            {
                var aggregateException = new AggregateException(e, context.Exception);
                _loggerFactory().LogError(aggregateException, "Unhandled exception occurred");
            }
        }

        private static int RetrieveWorkspaceId(ExceptionLoggerContext context)
        {
            var workspaceUrlParameter = context.Request.GetRouteData().Values["workspaceID"] as string;
            int workspaceId;
            int.TryParse(workspaceUrlParameter, out workspaceId);
            return workspaceId;
        }

        private static ErrorModel CreateErrorModel(ExceptionLoggerContext context, int workspaceId)
        {
            string corrrelationId = string.Empty;
            IEnumerable<string> headerValues = Enumerable.Empty<string>();
            if (context.Request?.Headers.TryGetValues(CorrelationIdHandler.WEB_CORRELATION_ID_HEADER_NAME, out headerValues) == true)
            {
                corrrelationId = headerValues.FirstOrDefault();
            }
            return new ErrorModel(context.Exception, true)
            {
                WorkspaceId = workspaceId,
                Location = context.Request?.RequestUri.PathAndQuery,
                CorrelationId = corrrelationId
            };
        }
    }
}