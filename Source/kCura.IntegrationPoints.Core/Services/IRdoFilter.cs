using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IRdoFilter
    {
        Task<IEnumerable<ObjectTypeDTO>> GetAllViewableRdos();
    }
}
