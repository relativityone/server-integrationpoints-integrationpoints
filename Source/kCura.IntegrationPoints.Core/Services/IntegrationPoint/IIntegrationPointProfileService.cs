using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public interface IIntegrationPointProfileService
    {
        IntegrationPointProfileDto Read(int artifactId);

        IntegrationPointProfileSlimDto ReadSlim(int artifactId);

        IList<IntegrationPointProfileSlimDto> ReadAllSlim();

        int SaveProfile(IntegrationPointProfileDto dto);
    }
}
