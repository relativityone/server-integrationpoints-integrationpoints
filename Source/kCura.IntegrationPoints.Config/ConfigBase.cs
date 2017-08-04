
using System.Collections;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Config
{
	public abstract class ConfigBase
	{
		protected IDictionary _instanceSettings;

		protected ConfigBase() :
			this(Manager.Instance.GetConfig(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION))
		{
		}

		internal ConfigBase(IDictionary instanceSettings)
		{
			_instanceSettings = instanceSettings;
			Manager.Settings.ConfigCacheTimeout = 1;
		}

		protected T GetValue<T>(string instanceSettingName, T defaultValue)
		{
			object instanceSetting = _instanceSettings[instanceSettingName];
			T value = ConfigHelper.GetValue(instanceSetting, defaultValue);
			return value;
		}
	}
}
