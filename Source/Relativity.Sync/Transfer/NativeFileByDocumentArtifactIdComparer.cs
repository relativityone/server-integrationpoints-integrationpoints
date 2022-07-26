using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
    internal sealed class NativeFileByDocumentArtifactIdComparer : IEqualityComparer<INativeFile>
    {
        public bool Equals(INativeFile x, INativeFile y)
        {
            return x?.DocumentArtifactId == y?.DocumentArtifactId;
        }

        public int GetHashCode(INativeFile obj)
        {
            return obj.DocumentArtifactId.GetHashCode();
        }
    }
}