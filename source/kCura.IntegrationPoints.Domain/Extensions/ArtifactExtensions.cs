using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Domain.Extensions
{
	public static class ArtifactExtensions
	{
		public static JsTreeItemWithParentIdDto ToTreeItemWithParentIdDTO(this Artifact artifact)
		{
			return new JsTreeItemWithParentIdDto
			{
				Id = artifact.ArtifactID.ToString(),
				ParentId = artifact.ParentArtifactID.ToString(),
				Text = artifact.Name
			};
		}
	}
}