using System;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal class SyncMetricsStub : ISyncMetrics
	{
		public void TimedOperation(string name, TimeSpan duration, CommandExecutionStatus status)
		{
			// Intentionally left empty
		}
	}
}