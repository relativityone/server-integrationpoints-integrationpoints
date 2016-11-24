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
		public string Name => IntegrationPointFieldGuids.Name;
	}

	public class IntegrationPointProfileFieldsConstants : IIntegrationPointBaseFieldsConstants
	{
		public string NextScheduledRuntimeUTC => IntegrationPointProfileFields.NextScheduledRuntimeUTC;
		public string LastRuntimeUTC => string.Empty;
		public string FieldMappings => IntegrationPointProfileFields.FieldMappings;
		public string EnableScheduler => IntegrationPointProfileFields.EnableScheduler;
		public string SourceConfiguration => IntegrationPointProfileFields.SourceConfiguration;
		public string DestinationConfiguration => IntegrationPointProfileFields.DestinationConfiguration;
		public string SourceProvider => IntegrationPointProfileFields.SourceProvider;
		public string ScheduleRule => IntegrationPointProfileFields.ScheduleRule;
		public string OverwriteFields => IntegrationPointProfileFields.OverwriteFields;
		public string DestinationProvider => IntegrationPointProfileFields.DestinationProvider;
		public string JobHistory => string.Empty;
		public string LogErrors => IntegrationPointProfileFields.LogErrors;
		public string EmailNotificationRecipients => IntegrationPointProfileFields.EmailNotificationRecipients;
		public string HasErrors => string.Empty;
		public string Type => IntegrationPointProfileFields.Type;
		public string Name => IntegrationPointProfileFields.Name;
	}
}