namespace Relativity.Sync.Configuration
{
    internal interface IImageJobStartMetricsConfiguration : IJobStartMetricsConfiguration
    {
        int[] ProductionImagePrecedence { get; }

        bool IncludeOriginalImageIfNotFoundInProductions { get; }
    }
}
