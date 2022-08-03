using System;
using System.Collections.Generic;
using Relativity.API.Foundation;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations.DTO
{
    internal class ArtifactRef : IArtifactRef
    {
        public int ArtifactID { get; set; }
        public IList<Guid> Guids { get; set; }

        public ArtifactRef()
        {
            Guids = new List<Guid>();
        }
    }
}