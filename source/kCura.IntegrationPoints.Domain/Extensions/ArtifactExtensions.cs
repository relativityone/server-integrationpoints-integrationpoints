using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Domain.Extensions
{
	public static class ArtifactExtensions
	{
		public static TreeItemWithParentIdDTO ToTreeItemWithParentIdDTO(this Artifact artifact)
		{
			return new TreeItemWithParentIdDTO
			{
				Id = artifact.ArtifactID.ToString(),
				ParentId = artifact.ParentArtifactID.ToString(),
				Text = artifact.Name
			};
		}
	}
}