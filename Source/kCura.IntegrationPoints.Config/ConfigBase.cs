using System;
using System.Collections;

namespace kCura.IntegrationPoints.Config
{
    public abstract class ConfigBase
    {
        protected readonly Lazy<IDictionary> _instanceSettings;
        
        public IInstanceSettingsProvider InstanceSettingsProvider { get; set; }

        protected ConfigBase()
        {
            InstanceSettingsProvider = new DefaultInstanceSettingsProvider();
            _instanceSettings = new Lazy<IDictionary>(() => InstanceSettingsProvider.GetInstanceSettings());
        }

        protected T GetValue<T>(string instanceSettingName)
        {
            object instanceSetting = _instanceSettings.Value[instanceSettingName];
            T value = InstanceSettingsProvider.GetValue<T>(instanceSetting);
            return value;
        }

        protected T GetValue<T>(string instanceSettingName, T defaultValue)
        {
            object instanceSetting = _instanceSettings.Value[instanceSettingName];
            T value = InstanceSettingsProvider.GetValue(instanceSetting, defaultValue);
            return value;
        }
    }
}
