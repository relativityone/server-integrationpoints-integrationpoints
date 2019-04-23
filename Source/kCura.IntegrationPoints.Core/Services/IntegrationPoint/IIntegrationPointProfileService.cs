using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public interface IIntegrationPointProfileService
	{
		IList<IntegrationPointProfile> GetAllRDOs();
		IList<IntegrationPointProfile> GetAllRDOsWithAllFields();
		string GetSourceOptions(int artifactId);
		FieldEntry GetIdentifierFieldEntry(int artifactId);
		IntegrationPointProfile ReadIntegrationPointProfile(int artifactId);
		IntegrationPointProfileModel ReadIntegrationPointProfileModel(int artifactId);
		IList<IntegrationPointProfileModel> ReadIntegrationPointProfiles();
		IEnumerable<string> GetRecipientEmails(int artifactId);
		int SaveIntegration(IntegrationPointProfileModel model);
		IList<IntegrationPointProfileModel> ReadIntegrationPointProfilesSimpleModel();
	}
}