using kCura.IntegrationPoints.Common.Monitoring.Messages;

namespace kCura.IntegrationPoints.Core.Monitoring.MessageSink.ExternalCalls
{
    internal class ExternalCallsJobSummaryMessage : JobMessageBase
    {
        public ExternalCallsJobSummaryMessage(JobMessageBase jobContext, string serviceType)
        {
            CorrelationID = jobContext.CorrelationID;
            JobID = jobContext.JobID;
            Provider = jobContext.Provider;
            UnitOfMeasure = jobContext.UnitOfMeasure;
            WorkspaceID = jobContext.WorkspaceID;

            ServiceType = serviceType;
            TotalCount = 0;
            TotalDuration = 0;
            FailedCount = 0;
            FailedDuration = 0;
        }

        public string ServiceType
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public long TotalCount
        {
            get { return Get<long>(); }
            set { Set(value); }
        }

        public long TotalDuration
        {
            get { return Get<long>(); }
            set { Set(value); }
        }

        public long FailedCount
        {
            get { return Get<long>(); }
            set { Set(value); }
        }

        public long FailedDuration
        {
            get { return Get<long>(); }
            set { Set(value); }
        }
    }
}
