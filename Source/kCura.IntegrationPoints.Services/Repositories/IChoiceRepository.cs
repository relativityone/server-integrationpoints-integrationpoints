using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IChoiceRepository
	{
		IList<OverwriteFieldsModel> GetOverwriteFieldChoices();
	}
}