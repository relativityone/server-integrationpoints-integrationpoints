using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal sealed class ChoiceWithChildInfo : Choice
	{
		public IList<ChoiceWithChildInfo> Children { get; }

		public ChoiceWithChildInfo(int artifactID, string name, IList<ChoiceWithChildInfo> children)
		{
			ArtifactID = artifactID;
			Name = name;
			Children = children;
		}
	}
}
