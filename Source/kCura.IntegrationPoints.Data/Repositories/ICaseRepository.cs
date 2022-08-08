using System;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface ICaseRepository : IDisposable
    {
        ICaseInfoDto Read(int caseArtifactId);
    }
}
