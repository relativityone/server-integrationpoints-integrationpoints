using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public interface IIntegrationPointProfileService
	{
		IntegrationPointProfile GetRdo(int artifactId);
		IList<IntegrationPointProfile> GetAllRDOs();
		string GetSourceOptions(int artifactId);
		FieldEntry GetIdentifierFieldEntry(int artifactId);
		IntegrationPointProfileModel ReadIntegrationPointProfile(int artifactId);
		IList<IntegrationPointProfileModel> ReadIntegrationPointProfiles();
		IEnumerable<FieldMap> GetFieldMap(int artifactId);
		IEnumerable<string> GetRecipientEmails(int artifactId);
		int SaveIntegration(IntegrationPointProfileModel model);
	}
}