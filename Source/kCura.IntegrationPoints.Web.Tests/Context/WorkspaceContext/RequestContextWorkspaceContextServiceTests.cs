using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using Moq;
using NUnit.Framework;
using System;
using System.Web;
using kCura.IntegrationPoints.Common.Context;

namespace kCura.IntegrationPoints.Web.Tests.Context.WorkspaceContext
{
    [TestFixture, Category("Unit")]
    public class RequestContextWorkspaceContextServiceTests
    {
        private Mock<IWorkspaceContext> _nextWorkspaceContextServiceMock;

        [SetUp]
        public void SetUp()
        {
            _nextWorkspaceContextServiceMock = new Mock<IWorkspaceContext>();
        }

        [Test]
        public void ShouldReturnWorkspaceIfHttpRequestContainsWorkspaceId()
        {
            //arrange
            const int workspaceId = 1019723;

            HttpRequestBase httpRequest = CreateHttpRequestMock();
            httpRequest = AddWorkspaceIdToRequestContext(httpRequest, workspaceId.ToString());

            RequestContextWorkspaceContextService sut = CreateSut(httpRequest);

            //act
            int result = sut.GetWorkspaceID();

            //assert
            result.Should().Be(workspaceId);
        }

        [Test]
        public void ShouldCallNextServiceIfHttpRequestDoesNotContainWorkspaceId()
        {
            //arrange
            const int workspaceId = 10148942;
            _nextWorkspaceContextServiceMock
                .Setup(x => x.GetWorkspaceID())
                .Returns(workspaceId);

            HttpRequestBase httpRequest = CreateHttpRequestMock();
            RequestContextWorkspaceContextService sut = CreateSut(httpRequest);

            //act
            int result = sut.GetWorkspaceID();

            //assert
            result.Should().Be(workspaceId);
            _nextWorkspaceContextServiceMock
                .Verify(x => x.GetWorkspaceID());
        }

        [Test]
        public void ShouldCallNextServiceIfHttpRequestContainsWorkspaceIdWhichCannotBeParsedToNumber()
        {
            //arrange
            const int workspaceId = 10148942;
            _nextWorkspaceContextServiceMock
                .Setup(x => x.GetWorkspaceID())
                .Returns(workspaceId);

            const string nonNumericWorkspaceId = "xyz";
            HttpRequestBase httpRequest = CreateHttpRequestMock();
            httpRequest = AddWorkspaceIdToRequestContext(httpRequest, nonNumericWorkspaceId);

            RequestContextWorkspaceContextService sut = CreateSut(httpRequest);

            //act
            int result = sut.GetWorkspaceID();

            //assert
            result.Should().Be(workspaceId);
            _nextWorkspaceContextServiceMock
                .Verify(x => x.GetWorkspaceID());
        }

        [Test]
        public void ShouldRethrowExceptionThrownByNextService()
        {
            //arrange
            var expectedException = new InvalidOperationException();
            _nextWorkspaceContextServiceMock
                .Setup(x => x.GetWorkspaceID())
                .Throws(expectedException);

            HttpRequestBase httpRequest = CreateHttpRequestMock();
            RequestContextWorkspaceContextService sut = CreateSut(httpRequest);
            
            Action getWorkspaceIdAction = () => sut.GetWorkspaceID();

            // act & assert
            getWorkspaceIdAction.ShouldThrow<InvalidOperationException>()
                .Which.Should().Be(expectedException);
        }

        private RequestContextWorkspaceContextService CreateSut(HttpRequestBase httpRequest)
        {
            return new RequestContextWorkspaceContextService(httpRequest, _nextWorkspaceContextServiceMock.Object);
        }

        private HttpRequestBase CreateHttpRequestMock()
        {
            var request = new HttpRequest(string.Empty, "http://test.org", string.Empty);
            var response = new HttpResponse(null);
            var httpContext = new HttpContext(request, response);
            var httpContextWrapper = new HttpContextWrapper(httpContext);
            return httpContextWrapper.Request;
        }

        private static HttpRequestBase AddWorkspaceIdToRequestContext(
            HttpRequestBase httpRequest,
            string workspaceId)
        {
            const string workspaceIdKey = "workspaceID";
            httpRequest.RequestContext.RouteData.Values.Add(workspaceIdKey, workspaceId);
            return httpRequest;
        }
    }
}
