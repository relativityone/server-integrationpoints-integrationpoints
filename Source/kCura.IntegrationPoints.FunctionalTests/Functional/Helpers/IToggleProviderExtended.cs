using Relativity.Toggles;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers
{
    internal interface IToggleProviderExtended : IToggleProvider
    {
        Task SetAsync(string name, bool enabled);
    }
}
