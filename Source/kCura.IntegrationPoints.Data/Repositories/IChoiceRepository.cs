using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IChoiceRepository
    {
        Task<IList<ChoiceWithParentInfoDto>> QueryChoiceWithParentInfoAsync(
            ICollection<ChoiceDto> choicesToQuery, 
            ICollection<ChoiceDto> allChoices);
    }
}
