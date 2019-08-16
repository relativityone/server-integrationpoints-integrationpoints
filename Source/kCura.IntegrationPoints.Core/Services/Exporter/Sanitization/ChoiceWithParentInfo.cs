using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal sealed class ChoiceWithParentInfo : Choice
	{
		public int? ParentArtifactID { get; }

		public ChoiceWithParentInfo(int? parentArtifactID, int artifactID, string name)
		{
			ParentArtifactID = parentArtifactID;
			ArtifactID = artifactID;
			Name = name;
		}
	}
}
