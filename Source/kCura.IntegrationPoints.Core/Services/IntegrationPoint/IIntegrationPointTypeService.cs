using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public interface IIntegrationPointTypeService
    {
        IList<IntegrationPointType> GetAllIntegrationPointTypes();

        IntegrationPointType GetIntegrationPointType(Guid guid);
    }
}