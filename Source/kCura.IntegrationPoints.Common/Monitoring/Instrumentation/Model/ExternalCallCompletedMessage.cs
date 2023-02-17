using kCura.IntegrationPoints.Common.Monitoring.Messages;

namespace kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model
{
    public class ExternalCallCompletedMessage : JobMessageBase
    {
        internal ExternalCallCompletedMessage()
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

        internal ExternalCallCompletedMessage SetJobContext(InstrumentationJobContext context)
        {
            CorrelationID = context.CorrelationId;
            JobID = context.JobId.ToString();
            WorkspaceID = context.WorkspaceId;
            return this;
        }

        internal ExternalCallCompletedMessage SetCallContext(InstrumentationServiceCallContext context)
        {
            ServiceType = context.ServiceType;
            ServiceName = context.ServiceName;
            OperationName = context.OperationName;
            return this;
        }

        internal ExternalCallCompletedMessage SetPropertiesForSuccess(long duration)
        {
            Duration = duration;
            HasFailed = false;
            FailureReason = string.Empty;
            return this;
        }

        internal ExternalCallCompletedMessage SetPropertiesForFailure(long duration, string failReason)
        {
            Duration = duration;
            HasFailed = true;
            FailureReason = failReason;
            return this;
        }

        public static ExternalCallCompletedMessage CreateSuccessMessage(InstrumentationJobContext jobContext, InstrumentationServiceCallContext callContext,
            long duration)
        {
            return new ExternalCallCompletedMessage()
                .SetJobContext(jobContext)
                .SetCallContext(callContext)
                .SetPropertiesForSuccess(duration);
        }

        public static ExternalCallCompletedMessage CreateFailureMessage(InstrumentationJobContext jobContext,
            InstrumentationServiceCallContext callContext,
            long duration, string failReason)
        {
            return new ExternalCallCompletedMessage()
                .SetJobContext(jobContext)
                .SetCallContext(callContext)
                .SetPropertiesForFailure(duration, failReason);
        }
    }
}
