namespace Relativity.Sync.Configuration
{
	internal interface INotificationConfiguration : IConfiguration
	{
		int JobStatusArtifactId { get; }
	}
}