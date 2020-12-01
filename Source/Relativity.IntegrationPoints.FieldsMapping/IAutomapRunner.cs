using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IAutomapRunner
	{
		IEnumerable<FieldMap> MapFields(IEnumerable<DocumentFieldInfo> sourceFields, IEnumerable<DocumentFieldInfo> destinationFields, 
			string destinationProviderGuid, int sourceWorkspaceArtifactId, bool matchOnlyIdentifiers = false);

		Task<IEnumerable<FieldMap>> MapFieldsFromSavedSearchAsync(IEnumerable<DocumentFieldInfo> sourceFields, IEnumerable<DocumentFieldInfo> destinationFields,
			string destinationProviderGuid, int sourceWorkspaceArtifactId, int savedSearchArtifactId);
	}
}