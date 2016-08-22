namespace kCura.IntegrationPoints.Config
{
	public interface IConfig
	{
		/// <summary>
		/// The web api path to be used by the import api.
		/// </summary>
		string WebApiPath { get; }

		/// <summary>
		/// Disables the validation of the native file locations.
		/// </summary>
		bool DisableNativeLocationValidation { get; }

		/// <summary>
		/// Disables the validation of native file types.
		/// </summary>
		bool DisableNativeValidation { get; }

		/// <summary>
		/// The batch size for all providers except the Relativity provider.
		/// </summary>
		int BatchSize { get; }

		/// <summary>
		/// The flag to determine the instance of Relativity
		/// </summary>
		bool IsCloudInstance { get; }

		/// <summary>
		/// The flag to Use EddsResource for storing scratch table.
		/// </summary>
		bool UseEDDSResource { get; }
	}
}