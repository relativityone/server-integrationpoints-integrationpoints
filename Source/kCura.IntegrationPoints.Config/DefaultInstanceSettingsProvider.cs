using System.Collections;
using kCura.Apps.Common.Config;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Config
{
    public class DefaultInstanceSettingsProvider : IInstanceSettingsProvider
    {
        public IDictionary GetInstanceSettings()
        {
            Manager.Settings.ConfigCacheTimeout = 1;
            return Manager.Instance.GetConfig(Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION);
        }

        public T GetValue<T>(object input)
        {
            return ConfigHelper.GetValue<T>(input);
        }

        public T GetValue<T>(object input, T defaultValue)
        {
            return ConfigHelper.GetValue(input, defaultValue);
        }
    }
}