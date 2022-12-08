using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public interface IIntegrationPointProfileService
    {
        IntegrationPointProfileDto Read(int artifactId);

        IntegrationPointProfileSlimDto ReadSlim(int artifactId);

        IList<IntegrationPointProfileSlimDto> ReadAllSlim();

        IList<IntegrationPointProfileDto> ReadAll();

        int SaveProfile(IntegrationPointProfileDto dto);

        void UpdateConfiguration(int profileArtifactId, string sourceConfiguration, string destinationConfiguration);
    }
}
