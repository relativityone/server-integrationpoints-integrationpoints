using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IFieldsRepository
	{
		Task<IEnumerable<DocumentFieldInfo>> GetAllDocumentFieldsAsync(int workspaceID);
		Task<IEnumerable<DocumentFieldInfo>> GetFieldsByArtifactsIdAsync(IEnumerable<string> artifactIDs, int workspaceID);
	}
}
