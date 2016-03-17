namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public interface IConfig
	{
		string WebApiPath { get; }

		bool DisableNativeLocationValidation { get; }

		bool DisableNativeValidation { get; }
	}
}