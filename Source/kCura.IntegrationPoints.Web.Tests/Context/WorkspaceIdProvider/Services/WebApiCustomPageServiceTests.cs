using System.Web;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceIdProvider.Services;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceIdProvider.Services
{
	public class WebApiCustomPageServiceTests
	{
		[Test]
		public void ShouldReturnWorkspaceIfHttpRequestContainsWorkspaceId()
		{
			//arrange
			const int workspaceId = 1019723;
			const string workspaceIdKey = "workspaceID";

			HttpContext httpContext = CreateHttpContextMock();
			httpContext.Request.RequestContext.RouteData.Values.Add(workspaceIdKey, workspaceId.ToString());
			HttpContext.Current = httpContext;

			var service = new WebApiCustomPageService();

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(workspaceId);
		}

		[Test]
		public void ShouldReturnZeroIfHttpRequestDoesNotContainWorkspaceId()
		{
			//arrange
			HttpContext.Current = CreateHttpContextMock();

			var service = new WebApiCustomPageService();

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}

		[Test]
		public void ShouldReturnZeroIfHttpRequestContainsWorkspaceIdWhichCannotBeParsedToNumber()
		{
			//arrange
			const string nonNumericWorkspaceId = "xyz";
			const string workspaceIdKey = "workspaceID";

			HttpContext httpContext = CreateHttpContextMock();
			httpContext.Request.RequestContext.RouteData.Values.Add(workspaceIdKey, nonNumericWorkspaceId);
			HttpContext.Current = httpContext;

			var service = new WebApiCustomPageService();

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}

		private HttpContext CreateHttpContextMock()
		{
			var request = new HttpRequest(string.Empty, "http://test.org", string.Empty);
			var response = new HttpResponse(null);
			var mock = new HttpContext(request, response);
			return mock;
		}
	}
}
