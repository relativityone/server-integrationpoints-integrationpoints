using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Web.Context.UserContext;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
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

namespace kCura.IntegrationPoints.Web.Tests.Infrastructure.MessageHandlers
{
	[TestFixture]
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
				logger,
				() => webCorrelationContextProvide,
				() => workspaceIdProvider,
				() => userContext
			)
			{
			}

			public Task<HttpResponseMessage> SendAyncInternal(HttpRequestMessage request, CancellationToken cancellationToken)
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
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			Guid expectedCorrelationId = request.GetCorrelationId();

			await _sut.SendAyncInternal(request, CancellationToken.None);

			_loggerMock.Verify(x =>
				x.LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WebRequestCorrelationId)}", expectedCorrelationId.ToString())
			);
		}

		[Test]
		public async Task ItShouldPushWorkspaceIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			int expectedWorkspaceId = 123;
			_workspaceContextMock
				.Setup(x => x.GetWorkspaceId())
				.Returns(expectedWorkspaceId);

			await _sut.SendAyncInternal(request, CancellationToken.None);

			_loggerMock.Verify(x =>
				x.LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WorkspaceId)}", expectedWorkspaceId.ToString())
			);
		}

		[Test]
		public void ItShouldRethrowExceptionWhileAccessingWorkspaceId()
		{
			var thrownException = new Exception();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			_workspaceContextMock
				.Setup(x => x.GetWorkspaceId())
				.Throws(thrownException);

			CorrelationContextCreationException rethrownException = Assert.ThrowsAsync<CorrelationContextCreationException>(async () =>
			{
				await _sut.SendAyncInternal(request, CancellationToken.None);
			});
			Assert.AreEqual(thrownException, rethrownException.InnerException);
		}

		[Test]
		public async Task ItShouldPushUserIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			int expectedUserId = 532;

			_userContextMock
				.Setup(x => x.GetUserID())
				.Returns(expectedUserId);

			await _sut.SendAyncInternal(request, CancellationToken.None);

			_loggerMock.Verify(x =>
				x.LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.UserId)}", expectedUserId.ToString())
			);
		}

		[Test]
		public void ItShouldRethrowExceptionWhileAccessingUserId()
		{
			var thrownException = new Exception();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");

			_userContextMock
				.Setup(x => x.GetUserID())
				.Throws(thrownException);

			CorrelationContextCreationException rethrownException = Assert.ThrowsAsync<CorrelationContextCreationException>(async () =>
			{
				await _sut.SendAyncInternal(request, CancellationToken.None);
			});
			Assert.AreEqual(thrownException, rethrownException.InnerException);
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
