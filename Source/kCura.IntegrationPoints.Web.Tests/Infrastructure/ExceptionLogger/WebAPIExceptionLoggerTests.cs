using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Web.Infrastructure.ExceptionLoggers;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Infrastructure.ExceptionLogger
{
    [TestFixture, Category("Unit")]
    public class WebAPIExceptionLoggerTests : TestBase
    {
        private IErrorService _errorService;
        private ISystemEventLoggingService _systemEventLoggingService;
        private WebAPIExceptionLogger _webAPIFilterException;

        [SetUp]
        public override void SetUp()
        {
            _errorService = Substitute.For<IErrorService>();
            _systemEventLoggingService = Substitute.For<ISystemEventLoggingService>();
            _webAPIFilterException = new WebAPIExceptionLogger(_errorService, _systemEventLoggingService);
        }

        [Test]
        public void ItShouldLogErrorWithCorrespondingExceptionAndWorkspaceId()
        {
            int expectedWorkspaceId = 479;
            string expectedErrorMessage = "example_message";
            var expectedException = new Exception(expectedErrorMessage);
            ExceptionLoggerContext exceptionLoggerContext = CreateExceptionLoggerContextMock(expectedException, expectedWorkspaceId.ToString());

            _webAPIFilterException.Log(exceptionLoggerContext);


            _errorService.Received(1).Log(Arg.Is<ErrorModel>(x => x.FullError == expectedException.ToString() &&
                                                                x.WorkspaceId == expectedWorkspaceId && x.Message == expectedErrorMessage));
        }

        [Test]
        public void ItShouldLogErrorEvenWhenWorkspaceIdIsInvalid()
        {
            object workspaceId = null;

            var exceptionLoggerContext = CreateExceptionLoggerContextMock(new Exception(), workspaceId);

            _webAPIFilterException.Log(exceptionLoggerContext);

            _errorService.Received(1).Log(Arg.Is<ErrorModel>(x => x.WorkspaceId == 0));
        }

        [Test]
        public void ItShouldAddSystemEventWhenErrorLoggingHasFailed()
        {
            var webException = new Exception("web_exception");
            var errorLoggingException = new Exception("error_logging_exception");

            var exceptionLoggerContext = CreateExceptionLoggerContextMock(webException, 1);

            _errorService.When(x => x.Log(Arg.Any<ErrorModel>())).Do(x => { throw errorLoggingException; });

            _webAPIFilterException.Log(exceptionLoggerContext);

            _systemEventLoggingService
                .Received(1)
                .WriteErrorEvent("Integration Points", nameof(WebAPIExceptionLogger),
                    Arg.Is<AggregateException>(x => x.InnerExceptions.Contains(webException) && x.InnerExceptions.Contains(errorLoggingException)));
        }

        private ExceptionLoggerContext CreateExceptionLoggerContextMock(Exception e, object workspaceId)
        {
            var dict = new Dictionary<string, object> {{"workspaceID", workspaceId}};

            var httpRouteData = Substitute.For<IHttpRouteData>();
            httpRouteData.Values.Returns(dict);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Relativity");
            httpRequestMessage.Properties.Add(HttpPropertyKeys.RequestContextKey, new HttpRequestContext
            {
                RouteData = httpRouteData
            });

            ExceptionContext exceptionContext = new ExceptionContext(e, new ExceptionContextCatchBlock("", true, false), httpRequestMessage);
            return new ExceptionLoggerContext(exceptionContext);
        }
    }
}