namespace Relativity.Sync.Configuration
{
	internal interface IValidationConfiguration : IConfiguration
	{
		string JobName { get; }
		string NotificationEmails { get; }

		string TypeIdentifier { get; }
		string SourceProviderIdentifier { get; }

		int SourceWorkspaceArtifactId { get; set; }
		int DestinationWorkspaceArtifactId { get; set; }
	}
}