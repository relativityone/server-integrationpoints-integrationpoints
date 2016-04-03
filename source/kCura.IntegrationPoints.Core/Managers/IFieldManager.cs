using System;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IFieldManager
	{
		bool FieldExists(Guid fieldGuid);
		int Create(ArtifactFieldDTO field);
	}
}