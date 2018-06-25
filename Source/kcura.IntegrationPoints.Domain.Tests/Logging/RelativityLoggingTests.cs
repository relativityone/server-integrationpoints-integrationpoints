using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using kCura.IntegrationPoints.Domain.Logging;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.Logging.Configuration;
using Relativity.Logging.Factory;

namespace kCura.IntegrationPoints.Agent.Tests
{
	/// <summary>
	/// This tests verify our assumptions regarding Relativity Logging. 
	/// If any of these test fails correlation context will probably stop working
	/// </summary>
	[TestFixture()]
	public class RelativityLoggerTests
	{
		[Test]
		[Ignore("It doesn't work after upgrade to Relativity.Logging 9.4.315")]
		public void ItShouldUseLogicalCallContextWhenPushPropertyWasUsed()
		{
			string slotName = SerilogContextRestorer.SerilogContextSlotName;
			var options = new LoggerOptions();
			var configuration = new LogConfiguration
			{
				LoggingEnabled = true,
				Sinks = new List<Sink> { new NullSink() }
			};
			var logger = LogFactory.GetLogger(options, configuration);

			using (logger.LogContextPushProperty("x", "value"))
			{
				Assert.IsNotNull(CallContext.LogicalGetData(slotName));
			}

		}

		[Test]
		public void ItShouldNotUseLogicalCallContextWhenPushPropertyWasNotUsed()
		{
			string slotName = SerilogContextRestorer.SerilogContextSlotName;
			var options = new LoggerOptions();
			var configuration = new LogConfiguration
			{
				LoggingEnabled = true,
				Sinks = new List<Sink> { new NullSink() }
			};
			var logger = LogFactory.GetLogger(options, configuration);

			Assert.IsNull(CallContext.LogicalGetData(slotName));
		}
	}
}
