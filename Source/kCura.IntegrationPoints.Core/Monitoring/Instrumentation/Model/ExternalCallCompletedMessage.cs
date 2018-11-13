using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Messages;

namespace kCura.IntegrationPoints.Core.Monitoring.Instrumentation.Model
{
	internal class ExternalCallCompletedMessage : JobMessageBase
	{
		public ExternalCallCompletedMessage()
		{
			UnitOfMeasure = UnitsOfMeasureConstants.MILLISECONDS;
		}

		public string ServiceType
		{
			get { return Get<string>(); }
			private set { Set(value); }
		}

		public string ServiceName
		{
			get { return Get<string>(); }
			private set { Set(value); }
		}

		public string OperationName
		{
			get { return Get<string>(); }
			private set { Set(value); }
		}

		public long Duration
		{
			get { return Get<long>(); }
			private set { Set(value); }
		}

		public bool HasFailed
		{
			get { return Get<bool>(); }
			private set { Set(value); }
		}

		public string FailureReason
		{
			get { return Get<string>(); }
			private set { Set(value); }
		}

		public ExternalCallCompletedMessage SetJobContext(InstrumentationJobContext context)
		{
			CorrelationID = context.CorrelationId;
			JobID = context.JobId.ToString();
			WorkspaceID = context.WorkspaceId;
			return this;
		}

		public ExternalCallCompletedMessage SetCallContext(InstrumentationServiceCallContext context)
		{
			ServiceType = context.ServiceType;
			ServiceName = context.ServiceName;
			OperationName = context.OperationName;
			return this;
		}

		public ExternalCallCompletedMessage SetPropertiesForSuccess(long duration)
		{
			Duration = duration;
			HasFailed = false;
			FailureReason = string.Empty;
			return this;
		}

		public ExternalCallCompletedMessage SetPropertiesForFailure(long duration, string failReason)
		{
			Duration = duration;
			HasFailed = true;
			FailureReason = failReason;
			return this;
		}
	}
}
