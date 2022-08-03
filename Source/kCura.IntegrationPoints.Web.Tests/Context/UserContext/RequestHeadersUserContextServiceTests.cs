using System;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Context.UserContext;
using Moq;
using NUnit.Framework;
using System.Collections.Specialized;
using System.Web;

namespace kCura.IntegrationPoints.Web.Tests.Context.UserContext
{
    [TestFixture, Category("Unit")]
    public class RequestHeadersUserContextServiceTests
    {
        private Mock<IUserContext> _nextUserContextServiceMock;

        private const string _USER_HEADER_VALUE = "X-IP-USERID";
        private const string _CASE_USER_HEADER_VALUE = "X-IP-CASEUSERID";

        [SetUp]
        public void SetUp()
        {
            _nextUserContextServiceMock = new Mock<IUserContext>();
        }

        [Test]
        public void GetUserID_ShouldReturnUserIDIfHeaderContainsValue()
        {
            //arrange
            const int userId = 1019723;
            var headers = new NameValueCollection
            {
                {_USER_HEADER_VALUE, userId.ToString() }
            };
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetUserID();

            //assert
            result.Should().Be(userId);
        }

        [Test]
        public void GetUserID_ShouldReturnFirstUserIDIfHeaderContainsMoreThanOneValue()
        {
            //arrange
            const int userId = 1019723;
            var headers = new NameValueCollection
            {
                {_USER_HEADER_VALUE, userId.ToString() },
                {_USER_HEADER_VALUE, "NAN" },
                {_USER_HEADER_VALUE, "5453423" }
            };
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetUserID();

            //assert
            result.Should().Be(userId);
        }

        [Test]
        public void GetUserID_ShouldCallNextServiceIfHeaderDoesNotContainValue()
        {
            //arrange
            const int userId = 1019723;
            var headers = new NameValueCollection();
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            _nextUserContextServiceMock.Setup(x => x.GetUserID()).Returns(userId);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetUserID();

            //assert
            result.Should().Be(userId);
            _nextUserContextServiceMock.Verify(x=>x.GetUserID());
        }

        [Test]
        public void GetUserID_ShouldCallNextServiceIfHeaderContainNonNumericValue()
        {
            //arrange
            const int userId = 1019723;
            var headers = new NameValueCollection
            {
                {_USER_HEADER_VALUE, "NAN" }
            };
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            _nextUserContextServiceMock.Setup(x => x.GetUserID()).Returns(userId);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetUserID();

            //assert
            result.Should().Be(userId);
            _nextUserContextServiceMock.Verify(x => x.GetUserID());
        }

        [Test]
        public void GetUserID_ShouldRethrowNextServiceException()
        {
            //arrange
            var expectedException = new InvalidOperationException();
            var headers = new NameValueCollection();
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            _nextUserContextServiceMock.Setup(x => x.GetUserID()).Throws(expectedException);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            Action getUserIDAction = () => sut.GetUserID();

            //assert
            getUserIDAction.ShouldThrow<InvalidOperationException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void GetWorkspaceUserID_ShouldReturnUserIDIfHeaderContainsValue()
        {
            //arrange
            const int caseUserId = 1019723;
            var headers = new NameValueCollection
            {
                {_CASE_USER_HEADER_VALUE, caseUserId.ToString() }
            };
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetWorkspaceUserID();

            //assert
            result.Should().Be(caseUserId);
        }

        [Test]
        public void GetWorkspaceUserID_ShouldReturnFirstUserIDIfHeaderContainsMoreThanOneValue()
        {
            //arrange
            const int caseUserId = 1019723;
            var headers = new NameValueCollection
            {
                {_CASE_USER_HEADER_VALUE, caseUserId.ToString() },
                {_CASE_USER_HEADER_VALUE, "NAN" },
                {_CASE_USER_HEADER_VALUE, "5453423" }
            };
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetWorkspaceUserID();

            //assert
            result.Should().Be(caseUserId);
        }

        [Test]
        public void GetWorkspaceUserID_ShouldCallNextServiceIfHeaderDoesNotContainValue()
        {
            //arrange
            const int caseUserId = 1019723;
            var headers = new NameValueCollection();
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            _nextUserContextServiceMock.Setup(x => x.GetWorkspaceUserID()).Returns(caseUserId);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetWorkspaceUserID();

            //assert
            result.Should().Be(caseUserId);
            _nextUserContextServiceMock.Verify(x => x.GetWorkspaceUserID());
        }

        [Test]
        public void GetWorkspaceUserID_ShouldCallNextServiceIfHeaderContainNonNumericValue()
        {
            //arrange
            const int caseUserId = 1019723;
            var headers = new NameValueCollection
            {
                {_CASE_USER_HEADER_VALUE, "NAN" }
            };
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            _nextUserContextServiceMock.Setup(x => x.GetWorkspaceUserID()).Returns(caseUserId);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            int result = sut.GetWorkspaceUserID();

            //assert
            result.Should().Be(caseUserId);
            _nextUserContextServiceMock.Verify(x => x.GetWorkspaceUserID());
        }

        [Test]
        public void GetWorkspaceUserID_ShouldRethrowNextServiceException()
        {
            //arrange
            var expectedException = new InvalidOperationException();
            var headers = new NameValueCollection();
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(x => x.Headers).Returns(headers);

            _nextUserContextServiceMock.Setup(x => x.GetWorkspaceUserID()).Throws(expectedException);

            RequestHeadersUserContextService sut = CreateSut(httpRequestMock.Object);

            //act
            Action getWorkspaceUserIDAction = () => sut.GetWorkspaceUserID();

            //assert
            getWorkspaceUserIDAction.ShouldThrow<InvalidOperationException>()
                .Which.Should().Be(expectedException);
        }

        private RequestHeadersUserContextService CreateSut(HttpRequestBase httpRequest)
        {
            return new RequestHeadersUserContextService(httpRequest, _nextUserContextServiceMock.Object);
        }
    }
}
