using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Models
{
    public class ObjectTypeDTO
    {
        public int ArtifactId { get; set; } 
        public int ParentArtifactId { get; set; }
        public int ParentArtifactTypeId { get; set; } 
        public int? DescriptorArtifactTypeId { get; set; } 
        public string Name { get; set; }
        public List<Guid> Guids { get; set; }
        public bool BelongsToApplication { get; set; }
    }
}