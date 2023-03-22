using System.Threading.Tasks;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Common.Toggles
{
    public interface IRipToggleProvider
    {
        Task<bool> IsEnabledAsync<T>() where T : IToggle;

        bool IsEnabled<T>() where T : IToggle;

        bool IsEnabledByName(string name);

        Task<bool> IsEnabledByNameAsync(string name);
    }
}
