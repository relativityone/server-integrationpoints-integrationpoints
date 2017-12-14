using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using kCura.IntegrationPoints.Domain.Logging;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Domain.Tests.Logging
{
	[TestFixture()]
	public class LogContextHelperTests
	{
		[Test]
		public void ItShouldUseDictionaryAsLogicalCallContextObjectForAgent() // so it is properly exchanged between app domains
		{
			AgentCorrelationContext correlationContext = GetAgentCorrelationContextTestData();

			using (LogContextHelper.CreateAgentLogContext(correlationContext)) // ACT
			{
				// ASSERT
				var contextObject = CallContext.LogicalGetData(LogContextHelper.AgentContextSlotName) as Dictionary<string, object>;
				Assert.IsNotNull(contextObject);
			}
		}

		[Test]
		public void ItShouldClearContextForAgentAfterDispose()
		{
			AgentCorrelationContext correlationContext = GetAgentCorrelationContextTestData();

			using (LogContextHelper.CreateAgentLogContext(correlationContext))
			{
				// ACT
			}

			// ASSERT
			object contextObject = CallContext.LogicalGetData(LogContextHelper.AgentContextSlotName);
			Assert.IsNull(contextObject);
		}

		[Test]
		public void ItShouldReturnCloneOfAgentCorrelationContext()
		{
			AgentCorrelationContext correlationContext = GetAgentCorrelationContextTestData();

			AgentCorrelationContext actualCorrelationContext;
			using (LogContextHelper.CreateAgentLogContext(correlationContext))
			{
				// ACT
				actualCorrelationContext = LogContextHelper.GetAgentLogContext();
			}

			// ASSERT
			Assert.AreNotSame(correlationContext, actualCorrelationContext);
			Assert.AreEqual(correlationContext.JobId, actualCorrelationContext.JobId);
			Assert.AreEqual(correlationContext.RootJobId, actualCorrelationContext.RootJobId);
			Assert.AreEqual(correlationContext.ActionName, actualCorrelationContext.ActionName);
			Assert.AreEqual(correlationContext.UserId, actualCorrelationContext.UserId);
			Assert.AreEqual(correlationContext.WorkspaceId, actualCorrelationContext.WorkspaceId);
		}

		[Test]
		public void ItShouldUseDictionaryAsLogicalCallContextObjectForWeb() // so it is properly exchanged between app domains
		{
			WebCorrelationContext correlationContext = GetWebCorrelationContextTestData();

			using (LogContextHelper.CreateWebLogContext(correlationContext)) // ACT
			{
				// ASSERT
				var contextObject = CallContext.LogicalGetData(LogContextHelper.WebContextSlotName) as Dictionary<string, object>;
				Assert.IsNotNull(contextObject);
			}
		}

		[Test]
		public void ItShouldClearContextForWebAfterDispose()
		{
			WebCorrelationContext correlationContext = GetWebCorrelationContextTestData();

			using (LogContextHelper.CreateWebLogContext(correlationContext))
			{
				// ACT
			}

			// ASSERT
			object contextObject = CallContext.LogicalGetData(LogContextHelper.WebContextSlotName);
			Assert.IsNull(contextObject);
		}

		[Test]
		public void ItShouldReturnCloneOfWebCorrelationContext()
		{
			WebCorrelationContext correlationContext = GetWebCorrelationContextTestData();

			WebCorrelationContext actualCorrelationContext;
			using (LogContextHelper.CreateWebLogContext(correlationContext))
			{
				// ACT
				actualCorrelationContext = LogContextHelper.GetWebLogContext();
			}

			// ASSERT
			Assert.AreNotSame(correlationContext, actualCorrelationContext);
			Assert.AreEqual(correlationContext.CorrelationId, actualCorrelationContext.CorrelationId);
			Assert.AreEqual(correlationContext.WebRequestCorrelationId, actualCorrelationContext.WebRequestCorrelationId);
			Assert.AreEqual(correlationContext.ActionName, actualCorrelationContext.ActionName);
			Assert.AreEqual(correlationContext.UserId, actualCorrelationContext.UserId);
			Assert.AreEqual(correlationContext.WorkspaceId, actualCorrelationContext.WorkspaceId);
		}

		private AgentCorrelationContext GetAgentCorrelationContextTestData()
		{
			return new AgentCorrelationContext
			{
				ActionName = "test",
				JobId = 134,
				UserId = 3,
				RootJobId = null,
				WorkspaceId = 45
			};
		}

		private WebCorrelationContext GetWebCorrelationContextTestData()
		{
			return new WebCorrelationContext
			{
				ActionName = "test",
				CorrelationId = Guid.NewGuid(),
				UserId = 3,
				WebRequestCorrelationId = Guid.NewGuid(),
				WorkspaceId = 45
			};
		}
	}
}
