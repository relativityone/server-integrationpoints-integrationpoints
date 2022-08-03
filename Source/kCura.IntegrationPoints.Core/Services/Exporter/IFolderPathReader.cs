using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public interface IFolderPathReader
    {
        void SetFolderPaths(List<ArtifactDTO> artifacts);
    }
}