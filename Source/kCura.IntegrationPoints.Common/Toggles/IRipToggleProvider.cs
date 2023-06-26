using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
    public interface IRipToggleProvider
    {
        bool IsEnabled<T>() where T : IToggle;

        bool IsEnabledByName(string name);
    }
}
