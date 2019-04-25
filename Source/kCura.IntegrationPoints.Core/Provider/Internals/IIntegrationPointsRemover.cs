using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public interface IIntegrationPointsRemover
    {
        void DeleteIntegrationPointsBySourceProvider(List<int> sourceProvidersIDs);
    }
}
