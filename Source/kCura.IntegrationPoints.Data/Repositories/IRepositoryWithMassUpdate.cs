using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IRepositoryWithMassUpdate
    {
        /// <summary>
        /// Updates given fields for given artifacts. For multi objects field merge option is used
        /// </summary>
        /// <param name="artifactIDsToUpdate"></param>
        /// <param name="fieldsToUpdate"></param>
        /// <returns></returns>
        Task<bool> MassUpdateAsync(
            IEnumerable<int> artifactIDsToUpdate,
            IEnumerable<FieldUpdateRequestDto> fieldsToUpdate);
    }
}
