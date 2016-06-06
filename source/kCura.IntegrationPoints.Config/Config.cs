using System;
using System.Collections;
using kCura.Apps.Common.Config;

namespace kCura.IntegrationPoints.Config
{
	public class Config : IConfig
	{
		private static readonly Lazy<Config> _instance = new Lazy<Config>(() => new Config());
		private static IDictionary _instanceSettings;
		private bool? _isCloudInstance;
		private bool? _useEddsResource;

		private const int _BATCH_SIZE_DEFAULT = 1000;
		private const string _DISABLE_NATIVE_LOCATION_VALIDATION = "DisableNativeLocationValidation";
		private const string _DISABLE_NATIVE_VALIDATION = "DisableNativeValidation";
		private const string _BATCH_SIZE = "BatchSize";

		protected Config() :
			this(Manager.Instance.GetConfig(Contracts.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION))
		{
		}

		public static Config Instance => _instance.Value;


		internal Config(IDictionary instanceSettings)
		{
			_instanceSettings = instanceSettings;
			kCura.Apps.Common.Config.Manager.Settings.ConfigCacheTimeout = 1;
		}

		public string WebApiPath => GetValue(Contracts.Constants.WEB_API_PATH, string.Empty);

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

		public bool IsCloudInstance
		{
			get
			{
				if (!_isCloudInstance.HasValue)
				{
					const string setting = "CloudInstance";
					var config = Manager.Instance.GetConfig("Relativity.Core");
					bool isCloudInstance = false;
					if (config.Contains(setting))
					{
						Boolean.TryParse(config[setting] as string, out isCloudInstance);
					}
					_isCloudInstance = isCloudInstance;
				}
				return _isCloudInstance.Value;
			}
		}

		public bool UseEDDSResource
		{
			get
			{
				if (!_useEddsResource.HasValue)
				{
					const string setting = "UseEDDSResource";
					var config = Manager.Instance.GetConfig("Relativity.Data");
					bool useEddsResource = true;
					if (config.Contains(setting))
					{
						Boolean.TryParse(config[setting] as string, out useEddsResource);
					}
					_useEddsResource = useEddsResource;
				}
				return _useEddsResource.Value;
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
