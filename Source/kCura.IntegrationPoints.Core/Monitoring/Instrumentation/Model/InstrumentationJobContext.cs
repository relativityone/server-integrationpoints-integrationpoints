using System;

namespace kCura.IntegrationPoints.Core.Monitoring.Instrumentation.Model
{
	[Serializable]
	public class InstrumentationJobContext
	{
		public long JobId { get; }
		public string CorrelationId { get; }
		public int WorkspaceId { get; }

		public InstrumentationJobContext(long jobId, string correlationId, int workspaceId)
		{
			JobId = jobId;
			CorrelationId = correlationId;
			WorkspaceId = workspaceId;
		}

		public static InstrumentationJobContext EmptyContext => new InstrumentationJobContext(0, "", 0);
	}
}
