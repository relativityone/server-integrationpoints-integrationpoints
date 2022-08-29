using System;
using kCura.EventHandler;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface IIntegrationPointViewPreLoad
    {
        void PreLoad(Artifact artifact);

        void ResetSavedSearch(Action<Artifact> initializeAction, Artifact artifact, IEHHelper helper);
    }
}
