﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Web.Context.WorkspaceContext;
using kCura.IntegrationPoints.Web.Infrastructure.MessageHandlers;
using kCura.IntegrationPoints.Web.IntegrationPointsServices.Logging;
using Moq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Infrastructure.MessageHandlers
{
	public class CorrelationIdHandlerTests : WebControllerTestBase
	{
		private Mock<IWorkspaceIdProvider> _workpsaceIdProviderMock;
		private CorrelationIdHandlerMock _subjectUnderTests;

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

		public class CorrelationIdHandlerMock : CorrelationIdHandler
		{
			public CorrelationIdHandlerMock(
				ICPHelper helper,
				IWebCorrelationContextProvider webCorrelationContextProvide,
				IWorkspaceIdProvider workspaceIdProvider
			) : base(
				helper,
				() => webCorrelationContextProvide,
				() => workspaceIdProvider
			)
			{
			}

			public Task<HttpResponseMessage> SendAyncInternal(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return SendAsync(request, cancellationToken);
			}
		}

		public override void SetUp()
		{
			base.SetUp();

			InitializeLoggerMockInHelper();
			IWebCorrelationContextProvider webCorrelationContextProviderMock = GetWebCorrelationContextProviderMock();
			_workpsaceIdProviderMock = new Mock<IWorkspaceIdProvider>();
			_subjectUnderTests = new CorrelationIdHandlerMock(Helper, webCorrelationContextProviderMock, _workpsaceIdProviderMock.Object)
			{
				InnerHandler = new MockHandler()
			};
		}

		[Test]
		public async Task ItShouldPushWebRequestCorrelationIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			Guid expectedCorrelationId = request.GetCorrelationId();

			await _subjectUnderTests.SendAyncInternal(request, CancellationToken.None);

			Logger.Received().LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WebRequestCorrelationId)}", expectedCorrelationId.ToString());
		}

		[Test]
		public async Task ItShouldPushWorkspaceIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			int expectedWorkspaceId = 123;
			_workpsaceIdProviderMock
				.Setup(x => x.GetWorkspaceId())
				.Returns(expectedWorkspaceId);

			await _subjectUnderTests.SendAyncInternal(request, CancellationToken.None);

			Logger.Received().LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WorkspaceId)}", expectedWorkspaceId.ToString());
		}

		[Test]
		public void ItShouldRethrowExceptionWhileAccessingWorkspaceId()
		{
			var thrownException = new Exception();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			_workpsaceIdProviderMock
				.Setup(x => x.GetWorkspaceId())
				.Throws(thrownException);

			CorrelationContextCreationException rethrownException = Assert.ThrowsAsync<CorrelationContextCreationException>(async () =>
			{
				await _subjectUnderTests.SendAyncInternal(request, CancellationToken.None);
			});
			Assert.AreEqual(thrownException, rethrownException.InnerException);
		}

		[Test]
		public async Task ItShouldPushUserIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			int expectedUserId = 532;

			var userInfo = Substitute.For<IUserInfo>();
			userInfo.ArtifactID.Returns(expectedUserId);
			var authenticationManager = Substitute.For<IAuthenticationMgr>();
			authenticationManager.UserInfo.Returns(userInfo);
			Helper.GetAuthenticationManager().Returns(authenticationManager);

			await _subjectUnderTests.SendAyncInternal(request, CancellationToken.None);

			Logger.Received().LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.UserId)}", expectedUserId.ToString());
		}

		[Test]
		public void ItShouldRethrowExceptionWhileAccessingUserId()
		{
			var thrownException = new Exception();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			Helper.GetAuthenticationManager().Throws(thrownException);

			CorrelationContextCreationException rethrownException = Assert.ThrowsAsync<CorrelationContextCreationException>(async () =>
			{
				await _subjectUnderTests.SendAyncInternal(request, CancellationToken.None);
			});
			Assert.AreEqual(thrownException, rethrownException.InnerException);
		}

		private void InitializeLoggerMockInHelper()
		{
			ILogFactory loggerFactory = Substitute.For<ILogFactory>();
			loggerFactory.GetLogger().Returns(Logger);
			Logger.ForContext<CorrelationIdHandler>().Returns(Logger);
			Helper.GetLoggerFactory().Returns(loggerFactory);
		}

		private static IWebCorrelationContextProvider GetWebCorrelationContextProviderMock()
		{
			IWebCorrelationContextProvider webCorrelationContextProviderMock = Substitute.For<IWebCorrelationContextProvider>();
			webCorrelationContextProviderMock.GetDetails(Arg.Any<string>(), Arg.Any<int>())
				.Returns(new WebActionContext(string.Empty, Guid.Empty));
			return webCorrelationContextProviderMock;
		}
	}
}
