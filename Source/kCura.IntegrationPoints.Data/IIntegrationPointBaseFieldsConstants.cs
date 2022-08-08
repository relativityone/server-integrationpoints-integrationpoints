namespace kCura.IntegrationPoints.Data
{
    public interface IIntegrationPointBaseFieldsConstants
    {
        string NextScheduledRuntimeUTC { get; }
        string LastRuntimeUTC { get; }
        string FieldMappings { get; }
        string EnableScheduler { get; }
        string SourceConfiguration { get; }
        string DestinationConfiguration { get; }
        string SourceProvider { get; }
        string ScheduleRule { get; }
        string OverwriteFields { get; }
        string DestinationProvider { get; }
        string JobHistory { get; }
        string LogErrors { get; }
        string EmailNotificationRecipients { get; }
        string HasErrors { get; }
        string Type { get; }
        string PromoteEligible { get; }
        string Name { get; }
    }

    public class IntegrationPointFieldsConstants : IIntegrationPointBaseFieldsConstants
    {
        public string NextScheduledRuntimeUTC => IntegrationPointFieldGuids.NextScheduledRuntimeUTC;
        public string LastRuntimeUTC => IntegrationPointFieldGuids.LastRuntimeUTC;
        public string FieldMappings => IntegrationPointFieldGuids.FieldMappings;
        public string EnableScheduler => IntegrationPointFieldGuids.EnableScheduler;
        public string SourceConfiguration => IntegrationPointFieldGuids.SourceConfiguration;
        public string DestinationConfiguration => IntegrationPointFieldGuids.DestinationConfiguration;
        public string SourceProvider => IntegrationPointFieldGuids.SourceProvider;
        public string ScheduleRule => IntegrationPointFieldGuids.ScheduleRule;
        public string OverwriteFields => IntegrationPointFieldGuids.OverwriteFields;
        public string DestinationProvider => IntegrationPointFieldGuids.DestinationProvider;
        public string JobHistory => IntegrationPointFieldGuids.JobHistory;
        public string LogErrors => IntegrationPointFieldGuids.LogErrors;
        public string EmailNotificationRecipients => IntegrationPointFieldGuids.EmailNotificationRecipients;
        public string HasErrors => IntegrationPointFieldGuids.HasErrors;
        public string Type => IntegrationPointFieldGuids.Type;
        public string PromoteEligible => IntegrationPointFieldGuids.PromoteEligible;
        public string Name => IntegrationPointFieldGuids.Name;
    }

    public class IntegrationPointProfileFieldsConstants : IIntegrationPointBaseFieldsConstants
    {
        public string NextScheduledRuntimeUTC => IntegrationPointProfileFieldGuids.NextScheduledRuntimeUTC;
        public string LastRuntimeUTC => string.Empty;
        public string FieldMappings => IntegrationPointProfileFieldGuids.FieldMappings;
        public string EnableScheduler => IntegrationPointProfileFieldGuids.EnableScheduler;
        public string SourceConfiguration => IntegrationPointProfileFieldGuids.SourceConfiguration;
        public string DestinationConfiguration => IntegrationPointProfileFieldGuids.DestinationConfiguration;
        public string SourceProvider => IntegrationPointProfileFieldGuids.SourceProvider;
        public string ScheduleRule => IntegrationPointProfileFieldGuids.ScheduleRule;
        public string OverwriteFields => IntegrationPointProfileFieldGuids.OverwriteFields;
        public string DestinationProvider => IntegrationPointProfileFieldGuids.DestinationProvider;
        public string JobHistory => string.Empty;
        public string LogErrors => IntegrationPointProfileFieldGuids.LogErrors;
        public string EmailNotificationRecipients => IntegrationPointProfileFieldGuids.EmailNotificationRecipients;
        public string HasErrors => string.Empty;
        public string Type => IntegrationPointProfileFieldGuids.Type;
        public string PromoteEligible => IntegrationPointProfileFieldGuids.PromoteEligible;
        public string Name => IntegrationPointProfileFieldGuids.Name;
    }
}