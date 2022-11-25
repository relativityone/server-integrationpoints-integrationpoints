using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public interface IIntegrationPointProfileService
    {
        IntegrationPointProfileDto Read(int artifactId);

        IntegrationPointProfileDto ReadSlim(int artifactId);

        IList<IntegrationPointProfileDto> ReadAllSlim();

        int SaveProfile(IntegrationPointProfileDto dto);

        void UpdateConfiguration(int profileArtifactId, string sourceConfiguration, string destinationConfiguration);
    }
}
