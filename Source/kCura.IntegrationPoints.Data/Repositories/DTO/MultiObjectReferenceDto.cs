using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
    public class MultiObjectReferenceDto : IFieldValueDto
    {
        public IReadOnlyCollection<int> ObjectReferences { get; }

        public object Value => ObjectReferences.Select(x => new RelativityObjectRef { ArtifactID = x }) .ToArray();

        public MultiObjectReferenceDto(int objectReference) : this(new[] { objectReference })
        { }

        public MultiObjectReferenceDto(IEnumerable<int> objectReferences)
        {
            ObjectReferences = objectReferences.ToArray();
        }
    }
}
