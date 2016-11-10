using System;
using System.Collections;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Config
{
	public class Config : IConfig
	{
		private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());
		private static IDictionary _instanceSettings;

		private const int _BATCH_SIZE_DEFAULT = 1000;
		private const string _DISABLE_NATIVE_LOCATION_VALIDATION = "DisableNativeLocationValidation";
		private const string _DISABLE_NATIVE_VALIDATION = "DisableNativeValidation";
		private const string _BATCH_SIZE = "BatchSize";

		protected Config() :
			this(Manager.Instance.GetConfig(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION))
		{
		}

		public static Config Instance => _instance.Value;

		internal Config(IDictionary instanceSettings)
		{
			_instanceSettings = instanceSettings;
			kCura.Apps.Common.Config.Manager.Settings.ConfigCacheTimeout = 1;
		}

		public string WebApiPath => GetValue(Constants.WEB_API_PATH, string.Empty);

		public bool DisableNativeLocationValidation => GetValue(_DISABLE_NATIVE_LOCATION_VALIDATION, false);

		public bool DisableNativeValidation => GetValue(_DISABLE_NATIVE_VALIDATION, false);

		public int BatchSize
		{
			get
			{
				var value = GetValue(_BATCH_SIZE, _BATCH_SIZE_DEFAULT);
				return value >= 0 ? value : _BATCH_SIZE_DEFAULT;
			}
		}

		private T GetValue<T>(string instanceSettingName, T defaultValue)
		{
			object instanceSetting = _instanceSettings[instanceSettingName];
			T value = ConfigHelper.GetValue(instanceSetting, defaultValue);
			return value;
		}
	}
}