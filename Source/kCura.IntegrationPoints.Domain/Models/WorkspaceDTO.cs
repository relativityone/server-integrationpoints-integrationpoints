using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Models
{
    [Serializable]
    public class WorkspaceDTO : IEquatable<WorkspaceDTO>
    {
        public int ArtifactId { get; set; }

        public string Name { get; set; }

        public bool Equals(WorkspaceDTO other)
        {
            return other != null && this.ArtifactId == other.ArtifactId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            WorkspaceDTO workspaceDto = obj as WorkspaceDTO;
            if (workspaceDto == null)
            {
                return false;
            }

            return Equals(workspaceDto);
        }

        public override int GetHashCode()
        {
            return ArtifactId;
        }
    }
}
