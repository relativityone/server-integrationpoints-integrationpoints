using System;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Config
{
	public class Config : ConfigBase, IConfig
	{
		private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());
		
		private const int _BATCH_SIZE_DEFAULT = 1000;
		private const string _DISABLE_NATIVE_LOCATION_VALIDATION = "DisableNativeLocationValidation";
		private const string _DISABLE_NATIVE_VALIDATION = "DisableNativeValidation";
		private const string _BATCH_SIZE = "BatchSize";

		protected Config()
		{
		}

		public static Config Instance => _instance.Value;

		public string WebApiPath => GetValue(Constants.WEB_API_PATH, string.Empty);

		public bool DisableNativeLocationValidation => GetValue(_DISABLE_NATIVE_LOCATION_VALIDATION, false);

		public bool DisableNativeValidation => GetValue(_DISABLE_NATIVE_VALIDATION, false);

		public int BatchSize
		{
			get
			{
				int value = GetValue(_BATCH_SIZE, _BATCH_SIZE_DEFAULT);
				return value >= 0 ? value : _BATCH_SIZE_DEFAULT;
			}
		}
	}
}