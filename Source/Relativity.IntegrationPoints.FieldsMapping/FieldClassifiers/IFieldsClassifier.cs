using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers
{
	public interface IFieldsClassifier
	{
		Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID);
	}
}