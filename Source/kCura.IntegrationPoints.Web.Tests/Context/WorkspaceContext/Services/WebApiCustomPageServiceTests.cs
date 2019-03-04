using System.Web;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext.Services;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceContext.Services
{
	public class WebApiCustomPageServiceTests
	{
		[Test]
		public void ShouldReturnWorkspaceIfHttpRequestContainsWorkspaceId()
		{
			//arrange
			const int workspaceId = 1019723;
			const string workspaceIdKey = "workspaceID";

			HttpRequestBase httpRequest = CreateHttpRequestMock();
			httpRequest.RequestContext.RouteData.Values.Add(workspaceIdKey, workspaceId.ToString());

			var service = new WebApiCustomPageService(httpRequest);

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(workspaceId);
		}

		[Test]
		public void ShouldReturnZeroIfHttpRequestDoesNotContainWorkspaceId()
		{
			//arrange
			HttpRequestBase httpRequest = CreateHttpRequestMock();

			var service = new WebApiCustomPageService(httpRequest);

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

			HttpRequestBase httpRequest = CreateHttpRequestMock();
			httpRequest.RequestContext.RouteData.Values.Add(workspaceIdKey, nonNumericWorkspaceId);

			var service = new WebApiCustomPageService(httpRequest);

			//act
			int result = service.GetWorkspaceID();

			//assert
			result.Should().Be(0);
		}

		private HttpRequestBase CreateHttpRequestMock()
		{
			var request = new HttpRequest(string.Empty, "http://test.org", string.Empty);
			return new HttpRequestWrapper(request);
		}
	}
}
