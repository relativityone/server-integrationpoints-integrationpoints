using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings
{
	public interface IFieldsClassifierRunner
	{
		Task<IList<FieldClassificationResult>> GetFilteredFieldsAsync(int workspaceID, IList<IFieldsClassifier> classifiers);
	}
}