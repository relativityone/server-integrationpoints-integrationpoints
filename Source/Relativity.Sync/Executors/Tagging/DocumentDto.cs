using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors.Tagging
{
    internal class DocumentDto : IEquatable<DocumentDto>
    {
        public int ArtifactId { get; set; }

        public string Identifier { get; set; }

        public bool Equals(DocumentDto other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ArtifactId == other.ArtifactId;
        }

        public override int GetHashCode()
        {
            return ArtifactId;
        }
    }
}
