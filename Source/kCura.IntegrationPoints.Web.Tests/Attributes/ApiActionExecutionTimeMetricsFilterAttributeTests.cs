using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Metrics;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Attributes
{
    [TestFixture]
    public class ApiActionExecutionTimeMetricsFilterAttributeTests
    {
        private Mock<IWindsorContainer> _containerFake;
        private Mock<IDateTimeHelper> _dateTimeHelperFake;
        private Mock<IControllerActionExecutionTimeMetrics> _controllerActionExecutionTimeMetricsMock;
        private const string _ITEM_KEY_NAME = "ApiActionExecutionStartTimestamp";

        [SetUp]
        public void SetUp()
        {
            _dateTimeHelperFake = new Mock<IDateTimeHelper>();
            _controllerActionExecutionTimeMetricsMock = new Mock<IControllerActionExecutionTimeMetrics>();

            _containerFake = new Mock<IWindsorContainer>();
            _containerFake.Setup(x => x.Resolve<IDateTimeHelper>()).Returns(_dateTimeHelperFake.Object);
            _containerFake.Setup(x => x.Resolve<IControllerActionExecutionTimeMetrics>()).Returns(_controllerActionExecutionTimeMetricsMock.Object);
        }

        [Test]
        public void OnActionExecuting_ShouldAddTimestampToActionContext()
        {
            // Arrange
            DateTime timestamp = DateTime.Now;
            _dateTimeHelperFake.Setup(x => x.Now()).Returns(timestamp);

            HttpActionContext context = PrepareActionExecutingContext();
            ApiActionExecutionTimeMetricsFilterAttribute sut = PrepareSut();

            // Act
            sut.OnActionExecuting(context);

            // Assert
            context.Request.Properties.ContainsKey(_ITEM_KEY_NAME).Should().BeTrue();
            context.Request.Properties[_ITEM_KEY_NAME].Should().BeAssignableTo<DateTime>().Which.Should().Be(timestamp);
        }

        [Test]
        public void OnActionExecuted_ShouldReportExecutionTime()
        {
            // Arrange
            DateTime startTime = DateTime.Now;
            const string rawUrl = "https://host.name/Relativity/App/RIP-GUID/api/MyApiAction?param=value";
            HttpMethod expectedMethod = HttpMethod.Get;

            HttpActionExecutedContext context = PrepareActionExecutedContext(startTime, rawUrl, expectedMethod);
            ApiActionExecutionTimeMetricsFilterAttribute sut = PrepareSut();

            // Act
            sut.OnActionExecuted(context);

            // Assert
            _controllerActionExecutionTimeMetricsMock.Verify(x => x.LogExecutionTime(
                It.Is<string>(url => url == "/api/MyApiAction"),
                It.Is<DateTime>(dateTime => dateTime == startTime),
                It.Is<string>(method => method == expectedMethod.Method)));
        }

        private ApiActionExecutionTimeMetricsFilterAttribute PrepareSut()
        {
            return new ApiActionExecutionTimeMetricsFilterAttribute()
            {
                Container = _containerFake.Object
            };
        }

        private HttpActionContext PrepareActionExecutingContext()
        {
            var context = new HttpActionContext();
            var request = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext
            {
                Request = request
            };
            context.ControllerContext = controllerContext;
            return context;
        }

        private HttpActionExecutedContext PrepareActionExecutedContext(DateTime startTime, string rawUrl, HttpMethod httpMethod)
        {
            HttpActionContext actionContext = PrepareActionExecutingContext();
            HttpRequestMessage request = new HttpRequestMessage(httpMethod, rawUrl);
            request.Properties.Add(_ITEM_KEY_NAME, startTime);
            actionContext.ControllerContext.Request = request;
            HttpActionExecutedContext actionExecutedContext = new HttpActionExecutedContext(actionContext, null);
            return actionExecutedContext;
        }
    }
}
