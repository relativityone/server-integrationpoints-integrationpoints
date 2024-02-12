namespace Relativity.Sync.Configuration
{
    internal interface IImageRetrieveConfiguration
    {
        int[] ProductionImagePrecedence { get; }

        bool IncludeOriginalImageIfNotFoundInProductions { get; }
    }
}
