using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Extensions
{
	[TestFixture]
	public class HttpConfigurationExtensionsTests
	{
		[Test]
		public void ShouldAddExceptionLoggerToServices()
		{
			// arrange
			var httpConfiguration = new HttpConfiguration();
			var exceptionLoggerMock = new Mock<IExceptionLogger>();

			// act
			httpConfiguration.AddExceptionLogger(exceptionLoggerMock.Object);

			// assert
			httpConfiguration.Services.GetServices(typeof(IExceptionLogger)).Should()
				.Contain(exceptionLoggerMock.Object, "because this exception logger was added");
		}

		[Test]
		public void ShouldAddMessageHandler()
		{
			// arrange
			var httpConfiguration = new HttpConfiguration();
			var messageHandlerMock = new Mock<DelegatingHandler>();

			// act
			httpConfiguration.AddMessageHandler(messageHandlerMock.Object);

			// assert
			httpConfiguration.MessageHandlers.Contains(messageHandlerMock.Object).Should()
				.BeTrue("because this message handler was added");
		}

		[Test]
		public void ShouldRegisterWebApiFilter()
		{
			// arrange
			var httpConfiguration = new HttpConfiguration();

			// act
			httpConfiguration.RegisterWebAPIFilters();

			// assert
			httpConfiguration
				.Filters
				.Where(x=>x.Instance.GetType() == typeof(LogApiExceptionFilterAttribute))
				.Should()
				.ContainSingle("because this filter was registered");
		}
	}
}
