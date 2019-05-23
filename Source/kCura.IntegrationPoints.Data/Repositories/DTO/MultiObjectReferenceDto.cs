using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Data.Repositories.DTO
{
	public class MultiObjectReferenceDto : IFieldValueDto
	{
		public IReadOnlyCollection<int> ObjectReferences { get; }

		public MultiObjectReferenceDto(int objectReference) : this(new[] { objectReference })
		{ }

		public MultiObjectReferenceDto(IEnumerable<int> objectReferences)
		{
			ObjectReferences = objectReferences.ToArray();
		}

		public object Value => ObjectReferences.ToArray();
	}
}
