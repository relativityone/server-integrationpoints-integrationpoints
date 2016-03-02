using System.Collections.Generic;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers
{
	public interface IFieldManager
	{
		ArtifactFieldDTO[] RetrieveLongTextFields(int rdoTypeId);
		ArtifactDTO[] RetrieveFields(int rdoTypeId, HashSet<string> fieldFieldsNames);
	}
}