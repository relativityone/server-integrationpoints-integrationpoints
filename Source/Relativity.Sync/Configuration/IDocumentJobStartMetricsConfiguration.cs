namespace Relativity.Sync.Configuration
{
	internal interface IDocumentJobStartMetricsConfiguration : IJobStartMetricsConfiguration
	{
		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }
	}
}
