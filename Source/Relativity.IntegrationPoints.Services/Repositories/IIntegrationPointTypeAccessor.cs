using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IIntegrationPointTypeAccessor
    {
        IList<IntegrationPointTypeModel> GetIntegrationPointTypes();
    }
}
