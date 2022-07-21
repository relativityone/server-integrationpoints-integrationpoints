using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal sealed class ChoiceWithChildInfo : Choice
    {
        public IList<ChoiceWithChildInfo> Children { get; }

        public ChoiceWithChildInfo(int artifactId, string name, IList<ChoiceWithChildInfo> children)
        {
            ArtifactID = artifactId;
            Name = name;
            Children = children;
        }
    }
}