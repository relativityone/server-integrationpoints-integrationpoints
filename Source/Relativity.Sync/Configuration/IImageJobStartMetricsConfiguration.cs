namespace Relativity.Sync.Configuration
{
	interface IImageJobStartMetricsConfiguration : ISumReporterConfiguration
	{
		int[] ProductionImagePrecedence { get; }

		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
