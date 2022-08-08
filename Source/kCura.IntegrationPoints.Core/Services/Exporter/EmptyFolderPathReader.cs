using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public class EmptyFolderPathReader : IFolderPathReader
    {
        public void SetFolderPaths(List<ArtifactDTO> artifacts)
        {
        }
    }
}