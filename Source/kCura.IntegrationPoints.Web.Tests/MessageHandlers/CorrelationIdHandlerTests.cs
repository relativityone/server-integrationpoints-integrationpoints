using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.MessageHandlers;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.MessageHandlers
{
	public class CorrelationIdHandlerTests : WebControllerTestBase
	{

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
			public CorrelationIdHandlerMock(ICPHelper helper) : base(helper)
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

			_subjectUnderTests = new CorrelationIdHandlerMock(Helper)
			{
				InnerHandler = new MockHandler()
			};
		}

		[Test]
		public void ItShouldHandleWebRequestId()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/Get");

			Guid correlationId = request.GetCorrelationId();

			HttpResponseMessage response = _subjectUnderTests.SendAyncInternal(request, CancellationToken.None).Result;

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			//Logger.Received().LogContextPushProperty("WebRequestId", correlationId);
		}
	}
}
