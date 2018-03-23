using System;
using System.Collections;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Config
{
	public abstract class ConfigBase
	{
		protected Lazy<IDictionary> _instanceSettings;

		protected ConfigBase()
		{
			_instanceSettings = new Lazy<IDictionary>(ReadInstanceSettings);
		}

		protected T GetValue<T>(string instanceSettingName, T defaultValue)
		{
			object instanceSetting = _instanceSettings.Value[instanceSettingName];
			T value = ConfigHelper.GetValue(instanceSetting, defaultValue);
			return value;
		}

		private IDictionary ReadInstanceSettings()
		{
			IDictionary instanceSettings = Manager.Instance.GetConfig(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION);
			Manager.Settings.ConfigCacheTimeout = 1;
			return instanceSettings;
		}
	}
}
