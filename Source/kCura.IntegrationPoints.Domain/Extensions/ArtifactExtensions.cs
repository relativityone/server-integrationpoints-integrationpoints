using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Domain.Extensions
{
    public static class ArtifactExtensions
    {
        public static JsTreeItemWithParentIdDTO ToTreeItemWithParentIdDTO(this Artifact artifact)
        {
            return new JsTreeItemWithParentIdDTO
            {
                Id = artifact.ArtifactID.ToString(),
                ParentId = artifact.ParentArtifactID.ToString(),
                Text = artifact.Name,
                Icon = JsTreeItemIconEnum.Folder.GetDescription()
            };
        }
    }
}