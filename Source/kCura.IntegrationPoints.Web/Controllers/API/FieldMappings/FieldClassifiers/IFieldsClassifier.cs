using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Web.Controllers.API.FieldMappings.FieldClassifiers
{
	public interface IFieldsClassifier
	{
		Task<IEnumerable<FieldClassificationResult>> ClassifyAsync(ICollection<RelativityObject> fields, int workspaceID);
	}
}