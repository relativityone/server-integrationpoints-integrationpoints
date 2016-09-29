using System;
using System.Linq;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public interface IArtifactTreeCreator : ITreeByParentIdCreator<Artifact>
    {
    }
}