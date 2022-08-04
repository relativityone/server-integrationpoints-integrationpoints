using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IIntegrationPointTypeRepository
    {
        IList<IntegrationPointTypeModel> GetIntegrationPointTypes();
    }
}