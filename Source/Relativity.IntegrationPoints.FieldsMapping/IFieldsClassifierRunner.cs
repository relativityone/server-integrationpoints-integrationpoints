using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IFieldsClassifierRunner
	{
		Task<IList<FieldClassificationResult>> GetFilteredFieldsAsync(int workspaceID);
		Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<DocumentFieldInfo> fields, int workspaceID);
		Task<IEnumerable<FieldClassificationResult>> ClassifyFieldsAsync(ICollection<string> artifactIDs, int workspaceID);
	}
}