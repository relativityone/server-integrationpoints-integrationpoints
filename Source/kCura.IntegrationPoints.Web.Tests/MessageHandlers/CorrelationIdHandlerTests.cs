using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Web.Logging;
using kCura.IntegrationPoints.Web.MessageHandlers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.MessageHandlers
{
	public class CorrelationIdHandlerTests : WebControllerTestBase
	{
		private CorrelationIdHandlerMock _subjectUnderTests;

		private IWebCorrelationContextProvider _webCorrelationContextProviderMock;
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
			public CorrelationIdHandlerMock(ICPHelper helper, IWebCorrelationContextProvider webCorrelationContextProvide) 
				: base(helper, webCorrelationContextProvide)
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

			_webCorrelationContextProviderMock = Substitute.For<IWebCorrelationContextProvider>();
			var loggerFactory = Substitute.For<ILogFactory>();
			loggerFactory.GetLogger().Returns(Logger);
			Logger.ForContext<CorrelationIdHandler>().Returns(Logger);
			Helper.GetLoggerFactory().Returns(loggerFactory);

			_subjectUnderTests = new CorrelationIdHandlerMock(Helper, _webCorrelationContextProviderMock)
			{
				InnerHandler = new MockHandler()
			};
		}

		[Test]
		public void ItShouldPushWebRequestCorrelationIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			Guid expectedCorrelationId = request.GetCorrelationId();

			HttpResponseMessage response = _subjectUnderTests.SendAyncInternal(request, CancellationToken.None).Result;

			Logger.Received().LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WebRequestCorrelationId)}", expectedCorrelationId.ToString());
		}

		[Test]
		public void ItShouldPushWorkspaceIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			int expectedWorkspaceId = 123;
			Helper.GetActiveCaseID().Returns(expectedWorkspaceId);

			HttpResponseMessage response = _subjectUnderTests.SendAyncInternal(request, CancellationToken.None).Result;

			Logger.Received().LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.WorkspaceId)}", expectedWorkspaceId.ToString());
		}

		[Test]
		public void ItShouldRethrowExceptionWhileAccessingWorkspaceId()
		{
			var thrownException = new Exception();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			Helper.GetActiveCaseID().Throws(thrownException);

			CorrelationContextCreationException rethrownException = Assert.Throws<CorrelationContextCreationException>(() =>
			{
				HttpResponseMessage response = _subjectUnderTests.SendAyncInternal(request, CancellationToken.None).Result;
			});
			Assert.AreEqual(thrownException, rethrownException.InnerException);
		}

		[Test]
		public void ItShouldPushUserIdToLogContext()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			int expectedUserId = 532;

			var userInfo = Substitute.For<IUserInfo>();
			userInfo.ArtifactID.Returns(expectedUserId);
			var authenticationManager = Substitute.For<IAuthenticationMgr>();
			authenticationManager.UserInfo.Returns(userInfo);
			Helper.GetAuthenticationManager().Returns(authenticationManager);

			HttpResponseMessage response = _subjectUnderTests.SendAyncInternal(request, CancellationToken.None).Result;

			Logger.Received().LogContextPushProperty($"RIP.{nameof(WebCorrelationContext.UserId)}", expectedUserId.ToString());
		}

		[Test]
		public void ItShouldRethrowExceptionWhileAccessingUserId()
		{
			var thrownException = new Exception();
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");
			Helper.GetAuthenticationManager().Throws(thrownException);

			CorrelationContextCreationException rethrownException = Assert.Throws<CorrelationContextCreationException>(() =>
			{
				HttpResponseMessage response = _subjectUnderTests.SendAyncInternal(request, CancellationToken.None).Result;
			});
			Assert.AreEqual(thrownException, rethrownException.InnerException);
		}
	}
}
