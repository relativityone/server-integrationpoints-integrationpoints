using System;

namespace kCura.IntegrationPoints.Core.Monitoring.Instrumentation.Model
{
	[Serializable]
	internal class InstrumentationJobContext
	{
		public string JobId { get; }
		public string CorrelationId { get; }
		public int WorkspaceId { get; }

		public InstrumentationJobContext(string jobId, string correlationId, int workspaceId)
		{
			JobId = jobId;
			CorrelationId = correlationId;
			WorkspaceId = workspaceId;
		}
	}
}
