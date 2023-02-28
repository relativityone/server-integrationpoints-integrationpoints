using FluentAssertions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Infrastructure.MessageHandlers;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Context;

namespace kCura.IntegrationPoints.Web.Tests.Infrastructure.MessageHandlers
{
    [TestFixture, Category("Unit")]
    public class CorrelationIdHandlerTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<IWorkspaceContext> _workspaceContextMock;
        private Mock<IUserContext> _userContextMock;
        private CorrelationIdHandlerMock _sut;

        /// <summary>
        /// We need to setup this dummy Handler to as CorrelationIdHandler will run the next in the chain message handler SyncAsync method
        /// </summary>
        private class MockHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return new TaskFactory<HttpResponseMessage>().StartNew(() => new HttpResponseMessage(HttpStatusCode.OK), cancellationToken);
            }
        }

        private class CorrelationIdHandlerMock : CorrelationIdHandler
        {
            public CorrelationIdHandlerMock(
                IAPILog logger,
                IWebCorrelationContextProvider webCorrelationContextProvide,
                IWorkspaceContext workspaceIdProvider,
                IUserContext userContext
            ) : base(
                () => logger,
                () => webCorrelationContextProvide,
                () => workspaceIdProvider,
                () => userContext
            )
            {
            }

            public Task<HttpResponseMessage> SendAsyncInternal(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return SendAsync(request, cancellationToken);
            }
        }

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _loggerMock.Setup(x => x.ForContext<CorrelationIdHandler>()).Returns(_loggerMock.Object);

            IWebCorrelationContextProvider webCorrelationContextProviderMock = GetWebCorrelationContextProviderMock();
            _workspaceContextMock = new Mock<IWorkspaceContext>();
            _userContextMock = new Mock<IUserContext>();

            _sut = new CorrelationIdHandlerMock(
                _loggerMock.Object,
                webCorrelationContextProviderMock,
                _workspaceContextMock.Object,
                _userContextMock.Object)
            {
                InnerHandler = new MockHandler()
            };
        }

        [Test]
        public async Task ItShouldPushWebRequestCorrelationIdToLogContext()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
            Guid expectedCorrelationId = request.GetCorrelationId();

            // act
            await _sut.SendAsyncInternal(request, CancellationToken.None).ConfigureAwait(false);

            // assert
            _loggerMock.Verify(x =>
                x.LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WebRequestCorrelationId)}", expectedCorrelationId.ToString())
            );
        }

        [Test]
        public async Task ItShouldPushWorkspaceIdToLogContext()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
            int expectedWorkspaceId = 123;
            _workspaceContextMock
                .Setup(x => x.GetWorkspaceID())
                .Returns(expectedWorkspaceId);

            // act
            await _sut.SendAsyncInternal(request, CancellationToken.None).ConfigureAwait(false);

            // assert
            _loggerMock.Verify(x =>
                x.LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WorkspaceId)}", expectedWorkspaceId.ToString())
            );
        }

        [Test]
        public void ItShouldRethrowExceptionWhileAccessingWorkspaceId()
        {
            // arrange
            var thrownException = new Exception();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
            _workspaceContextMock
                .Setup(x => x.GetWorkspaceID())
                .Throws(thrownException);

            Func<Task> sendAsyncAction = () => _sut.SendAsyncInternal(request, CancellationToken.None);

            // act & assert
            sendAsyncAction.ShouldThrow<CorrelationContextCreationException>()
                .Which.InnerException.Should().Be(thrownException);
        }

        [Test]
        public async Task ItShouldPushUserIdToLogContext()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
            int expectedUserId = 532;

            _userContextMock
                .Setup(x => x.GetUserID())
                .Returns(expectedUserId);

            // act
            await _sut.SendAsyncInternal(request, CancellationToken.None).ConfigureAwait(false);

            // assert
            _loggerMock.Verify(x =>
                x.LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.UserId)}", expectedUserId.ToString())
            );
        }

        [Test]
        public void ItShouldRethrowExceptionWhileAccessingUserId()
        {
            // arrange
            var thrownException = new Exception();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");

            _userContextMock
                .Setup(x => x.GetUserID())
                .Throws(thrownException);

            Func<Task> sendAsyncAction = () => _sut.SendAsyncInternal(request, CancellationToken.None);

            // act & assert
            sendAsyncAction.ShouldThrow<CorrelationContextCreationException>()
                .Which.InnerException.Should().Be(thrownException);
        }

        private static IWebCorrelationContextProvider GetWebCorrelationContextProviderMock()
        {
            var webCorrelationContextProviderMock = new Mock<IWebCorrelationContextProvider>();
            webCorrelationContextProviderMock
                .Setup(x => x.GetDetails(
                    It.IsAny<string>(),
                    It.IsAny<int>())
                )
                .Returns(new WebActionContext(string.Empty, Guid.Empty));

            return webCorrelationContextProviderMock.Object;
        }
    }
}
