using System.Collections.Generic;
using kCura.EventHandler;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface IRelativityProviderConfiguration
    {
        void UpdateNames(IDictionary<string, object> settings, Artifact artifact);
    }
}