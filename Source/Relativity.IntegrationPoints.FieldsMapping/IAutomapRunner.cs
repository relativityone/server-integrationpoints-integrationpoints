using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IAutomapRunner
	{
		IEnumerable<FieldMap> MapFields(IEnumerable<DocumentFieldInfo> sourceFields,
			IEnumerable<DocumentFieldInfo> destinationFields, bool matchOnlyIdentifiers = false);
	}
}