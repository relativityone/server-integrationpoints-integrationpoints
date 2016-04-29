namespace kCura.IntegrationPoints.Config
{
	public interface IConfig
	{
		string WebApiPath { get; }

		bool DisableNativeLocationValidation { get; }

		bool DisableNativeValidation { get; }

		int BatchSize { get; }
	}
}