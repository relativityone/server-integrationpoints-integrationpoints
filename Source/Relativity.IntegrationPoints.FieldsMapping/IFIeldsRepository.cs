using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IFieldsRepository
	{
		Task<IEnumerable<DocumentFieldInfo>> GetAllDocumentFieldsAsync(int workspaceId);
		Task<IEnumerable<DocumentFieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIds, int workspaceId);
	}
}
