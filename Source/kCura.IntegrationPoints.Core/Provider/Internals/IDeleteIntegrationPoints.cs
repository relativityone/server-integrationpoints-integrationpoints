using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public interface IDeleteIntegrationPoints
    {
        void DeleteIPsWithSourceProvider(List<int> sourceProvider);
    }
}
