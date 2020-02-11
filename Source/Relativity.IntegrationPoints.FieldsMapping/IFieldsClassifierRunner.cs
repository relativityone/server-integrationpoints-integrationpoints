using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public interface IFieldsClassifierRunner
	{
		Task<IList<FieldClassificationResult>> GetFilteredFieldsAsync(int workspaceID, IList<IFieldsClassifier> classifiers);
	}
}