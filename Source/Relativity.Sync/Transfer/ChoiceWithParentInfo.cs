using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class ChoiceWithParentInfo : Choice
	{
		public int? ParentArtifactId { get; }
		public List<ChoiceWithParentInfo> Children { get; }

		public ChoiceWithParentInfo(int? parentArtifactId, int artifactId, string name)
		{
			ParentArtifactId = parentArtifactId;
			ArtifactID = artifactId;
			Name = name;

			Children = new List<ChoiceWithParentInfo>();
		}
	}
}