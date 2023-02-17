using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Metrics;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Attributes
{
    [TestFixture]
    public class MvcActionExecutionTimeMetricsFilterAttributeTests
    {
        private Mock<IWindsorContainer> _containerFake;
        private Mock<IDateTimeHelper> _dateTimeHelperFake;
        private Mock<IControllerActionExecutionTimeMetrics> _controllerActionExecutionTimeMetricsMock;
        private const string _ITEM_KEY_NAME = "MvcActionExecutionStartTimestamp";

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

            ActionExecutingContext context = PrepareActionExecutingContext();
            MvcActionExecutionTimeMetricsFilterAttribute sut = PrepareSut();

            // Act
            sut.OnActionExecuting(context);

            // Assert
            context.HttpContext.Items.Contains(_ITEM_KEY_NAME).Should().BeTrue();
            context.HttpContext.Items[_ITEM_KEY_NAME].Should().BeAssignableTo<DateTime>().Which.Should().Be(timestamp);
        }

        [Test]
        public void OnResultExecuted_ShouldReportExecutionTime()
        {
            // Arrange
            DateTime startTime = DateTime.Now;
            const string rawUrl = "/Relativity/App/RIP-GUID/MyAction?param=value";
            const string expectedMethod = "GET";

            ResultExecutedContext context = PrepareResultExecutedContext(startTime, rawUrl, expectedMethod);
            MvcActionExecutionTimeMetricsFilterAttribute sut = PrepareSut();

            // Act
            sut.OnResultExecuted(context);

            // Assert
            _controllerActionExecutionTimeMetricsMock.Verify(x => x.LogExecutionTime(
                It.Is<string>(url => url == "/Relativity/App/RIP-GUID/MyAction"),
                It.Is<DateTime>(dateTime => dateTime == startTime),
                It.Is<string>(method => method == expectedMethod)));
        }

        private MvcActionExecutionTimeMetricsFilterAttribute PrepareSut()
        {
            return new MvcActionExecutionTimeMetricsFilterAttribute()
            {
                Container = _containerFake.Object
            };
        }

        private ActionExecutingContext PrepareActionExecutingContext()
        {
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            Dictionary<string, object> items = new Dictionary<string, object>();
            httpContext.SetupGet(x => x.Items).Returns(items);

            ActionExecutingContext context = new ActionExecutingContext()
            {
                HttpContext = httpContext.Object
            };

            return context;
        }

        private ResultExecutedContext PrepareResultExecutedContext(DateTime startTime, string rawUrl, string httpMethod)
        {
            Dictionary<string, object> items = new Dictionary<string, object>()
            {
                { _ITEM_KEY_NAME, startTime }
            };

            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(x => x.Items).Returns(items);
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.RawUrl).Returns(rawUrl);
            request.SetupGet(x => x.HttpMethod).Returns(httpMethod);
            httpContext.SetupGet(x => x.Request).Returns(request.Object);

            ResultExecutedContext context = new ResultExecutedContext()
            {
                HttpContext = httpContext.Object
            };

            return context;
        }
    }
}
