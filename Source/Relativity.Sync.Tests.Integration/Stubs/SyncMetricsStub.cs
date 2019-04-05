using System;
using System.Collections.Generic;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal class SyncMetricsStub : ISyncMetrics
	{
		public void TimedOperation(string name, TimeSpan duration, ExecutionStatus status)
		{
			// Intentionally left empty
		}

		public void TimedOperation(string name, TimeSpan duration, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			// Intentionally left empty
		}

		public void CountOperation(string name, ExecutionStatus status)
		{
			// Intentionally left empty
		}
	}
}