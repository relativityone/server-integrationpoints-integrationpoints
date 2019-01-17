using System;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	public class SyncMetricsStub : ISyncMetrics
	{
		public void TimedOperation(string name, TimeSpan duration, string executionStatus)
		{
			// Intentionally left empty
		}
	}
}