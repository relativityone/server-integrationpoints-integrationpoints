using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
	internal interface IChoiceCache
	{
		Task<IList<ChoiceWithParentInfoDto>> GetChoicesWithParentInfoAsync(ICollection<ChoiceDto> choices);
	}
}
