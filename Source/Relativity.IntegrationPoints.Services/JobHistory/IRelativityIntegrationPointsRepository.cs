using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public interface IRelativityIntegrationPointsRepository
    {
        List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> RetrieveIntegrationPoints();
    }
}