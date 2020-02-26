using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public interface IFieldsClassifier
	{
		Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<DocumentFieldInfo> fields, int workspaceID);
	}
}