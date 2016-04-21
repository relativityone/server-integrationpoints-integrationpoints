using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IIntegrationPointService
	{
		Data.IntegrationPoint GetRdo(int rdoID);
		string GetSourceOptions(int artifactID);
		FieldEntry GetIdentifierFieldEntry(int artifactID);
		IntegrationModel ReadIntegrationPoint(int artifactID);
		IEnumerable<FieldMap> GetFieldMap(int artifactID);
		int SaveIntegration(IntegrationModel model);
		IEnumerable<string> GetRecipientEmails(int integrationPoint);
	}
}