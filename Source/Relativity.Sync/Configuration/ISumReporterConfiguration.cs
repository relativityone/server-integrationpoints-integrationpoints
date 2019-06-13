namespace Relativity.Sync.Configuration
{
	internal interface ISumReporterConfiguration : IConfiguration
	{
		string WorkflowId { get; }
	}
}