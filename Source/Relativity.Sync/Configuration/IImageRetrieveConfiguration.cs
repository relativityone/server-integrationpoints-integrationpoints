namespace Relativity.Sync.Configuration
{
	internal interface IImageRetrieveConfiguration
	{
		int[] ProductionIds { get; }
		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
