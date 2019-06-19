using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
	internal sealed class ChoiceWithParentInfo : Choice
	{
		public int ParentArtifactID { get; }
		public IList<ChoiceWithParentInfo> Children { get; set; } = new List<ChoiceWithParentInfo>();

		public ChoiceWithParentInfo(int parentArtifactId, int artifactId, string name)
		{
			ParentArtifactID = parentArtifactId;
			ArtifactID = artifactId;
			Name = name;
		}
	}
}