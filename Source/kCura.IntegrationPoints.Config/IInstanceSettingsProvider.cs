using System.Collections;

namespace kCura.IntegrationPoints.Config
{
    public interface IInstanceSettingsProvider
    {
        IDictionary GetInstanceSettings();
        T GetValue<T>(object input);
        T GetValue<T>(object input, T defaultValue);
    }
}
